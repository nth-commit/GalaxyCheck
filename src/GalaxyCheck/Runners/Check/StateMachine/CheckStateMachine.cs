using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Internal;
using GalaxyCheck.Runners.Check.StateMachine.States;
using GalaxyCheck.Runners.Replaying;

namespace GalaxyCheck.Runners.Check.StateMachine
{
    internal class CheckStateMachine<Input, Test, TestRunExploration>
        where Test : Property.TestInput<Input>
    {
        private readonly ICheckStateMachineDriver<Input, Test> _driver;

        private readonly List<StateAudit> _audits = new();
        private CheckStateData<Test> _stateData = null!;

        private record StateAudit(CheckState<Test> State,
            CheckStateData<Test> StateData,
            CheckIteration<Input>? CheckIteration);

        public CheckState<Test> State { get; private set; } = null!;

        public CheckStateMachine(
            ICheckStateMachineDriver<Input, Test> driver,
            CheckState<Test> initialState,
            CheckStateData<Test> initialData)
        {
            _driver = driver;
            Update(initialState, initialData);
        }

        public CheckResult<Input> Result
        {
            get
            {
                var finalAudit = _audits.Last();
                if (finalAudit.State is not TerminalState<Test> terminalState)
                {
                    throw new Exception("Fatal: Check did not terminate");
                }

                var checks = _audits.Select(a => a.CheckIteration).OfType<CheckIteration<Input>>().ToList();

                return new CheckResult<Input>(
                    Iterations: finalAudit.StateData.CompletedIterationsUntilCounterexample,
                    Discards: finalAudit.StateData.Discards,
                    Shrinks:
                    finalAudit.StateData.TotalShrinks +
                    finalAudit.StateData.CompletedIterationsAfterCounterexample,
                    Counterexample: PickFinalCounterexample(finalAudit.StateData),
                    Checks: checks,
                    InitialParameters: _audits.First().StateData.NextParameters,
                    NextParameters: finalAudit.StateData.NextParameters,
                    TerminationReason: terminalState.Reason);
            }
        }

        public void Transition()
        {
            if (State is not TransitionableCheckState<Test> transitionableState)
            {
                throw new Exception($"Fatal: Cannot transition from a non-transitionable state. State: {State.GetType().Name}");
            }

            var (nextState, nextStateData) = transitionableState.Transition(_stateData);
            Update(nextState, nextStateData);
        }

        public void Jump(CheckState<Test> state) => Update(state, _stateData);

        private void Update(CheckState<Test> state, CheckStateData<Test> stateData)
        {
            State = state;
            _stateData = stateData;
            _audits.Add(new StateAudit(state, stateData, MapToCheckIterationOrIgnore(state)));
        }

        private CheckIteration<Input>? MapToCheckIterationOrIgnore(CheckState<Test> state)
        {
            return state switch
            {
                HoldingTestCounterexampleState<Test, TestRunExploration> s => MapStateToCheckIteration(s),
                HoldingTestNonCounterexampleState<Test, TestRunExploration> s => MapStateToCheckIteration(s),
                HoldingErrorState<Test> s => throw new Exceptions.GenErrorException(s.Description, s.ReplayParameters),
                _ => null
            };
        }

        private CheckIteration<Input> MapStateToCheckIteration(
            HoldingTestCounterexampleState<Test, TestRunExploration> s)
        {
            return new CheckIteration<Input>(
                PresentedInput: ResolveTestInput(s.Counterexample.ExampleSpace).PresentedInput,
                ExampleSpace: ResolveInputExampleSpace(s.Counterexample.ExampleSpace),
                Parameters: s.TestRunExplorationStateData.Instance.ReplayParameters,
                Path: s.Counterexample.Path,
                Exception: s.Counterexample.Exception);
        }

        private CheckIteration<Input> MapStateToCheckIteration(
            HoldingTestNonCounterexampleState<Test, TestRunExploration> s)
        {
            return new CheckIteration<Input>(
                PresentedInput: ResolveTestInput(s.NonCounterexample.ExampleSpace).PresentedInput,
                ExampleSpace: ResolveInputExampleSpace(s.NonCounterexample.ExampleSpace),
                Parameters: s.TestRunExplorationStateData.Instance.ReplayParameters,
                Path: s.NonCounterexample.Path,
                Exception: null);
        }

        private IExampleSpace<Input> ResolveInputExampleSpace(IExampleSpace<Test> testExampleSpace)
        {
            return testExampleSpace.MapExamples(ex => new Example<Input>(
                ex.Id,
                _driver.TestToInput(ex.Value).Input,
                ex.Distance));
        }

        private Property.TestInput<Input> ResolveTestInput(IExampleSpace<Test> exampleSpace)
        {
            return _driver.TestToInput(exampleSpace.Current.Value);
        }

        private static Counterexample<Input>? PickFinalCounterexample(CheckStateData<Test> stateData)
        {
            var counterexample = stateData.Counterexample;
            if (counterexample is null) return null;

            var replay = new Replay(counterexample.ReplayParameters, counterexample.Path);
            var replayEncoded = ReplayEncoding.Encode(replay);

            return new Counterexample<Input>(
                counterexample.ExampleSpace.Current.Id,
                counterexample.ExampleSpace.Current.Value.Input,
                counterexample.ExampleSpace.Current.Distance,
                counterexample.ReplayParameters,
                counterexample.Path,
                replayEncoded,
                counterexample.Exception,
                counterexample.ExampleSpace.Current.Value.PresentedInput);
        }
    }

    internal interface ICheckStateMachineDriver<out Input, in Test>
    {
        Property.TestInput<Input> TestToInput(Test test);
    }

    internal class SyncCheckStateMachineDriver<T> : ICheckStateMachineDriver<T, Property.Test<T>>
    {
        public Property.TestInput<T> TestToInput(Property.Test<T> test)
        {
            return test;
        }
    }

    internal class AsyncCheckStateMachineDriver<T> : ICheckStateMachineDriver<T, Property.AsyncTest<T>>
    {
        public Property.TestInput<T> TestToInput(Property.AsyncTest<T> test)
        {
            return test;
        }
    }
}
