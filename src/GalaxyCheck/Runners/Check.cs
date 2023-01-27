using System;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Gens.Parameters.Internal;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Runners.Check;
using System.Collections.Generic;
using System.Threading.Tasks;
using GalaxyCheck.Internal;
using GalaxyCheck.Runners.Check.StateMachine;
using GalaxyCheck.Runners.Check.StateMachine.States;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        public static CheckResult<T> Check<T>(
            this IGen<Property.Test<T>> property,
            int? iterations = null,
            int? seed = null,
            int? size = null,
            int? shrinkLimit = null,
            string? replay = null,
            bool deepCheck = true)
        {
            var stateMachineDriver = new SyncCheckStateMachineDriver<T>();
            var stateMachine = CreateStateMachine<T, Property.Test<T>, IEnumerable<ExplorationStage<Property.Test<T>>>>(
                stateMachineDriver, property, iterations, seed, size, shrinkLimit, replay, deepCheck);

            while (stateMachine.State is not TerminalState<Property.Test<T>>)
            {
                if (stateMachine.State is InterruptedCheckState<Property.Test<T>>)
                {
                    switch (stateMachine.State)
                    {
                        case HoldingInstanceState<Property.Test<T>> holdingInstanceState:
                        {
                            var instance = holdingInstanceState.Instance;

                            var initialTestRunExploration = instance.ExampleSpace.Explore(CheckTest<T>());

                            var nextState = new BeginTestRunExplorationState<Property.Test<T>, IEnumerable<ExplorationStage<Property.Test<T>>>>(
                                instance,
                                initialTestRunExploration);

                            stateMachine.Jump(nextState);
                            break;
                        }
                        case HoldingTestRunExplorationState<Property.Test<T>, IEnumerable<ExplorationStage<Property.Test<T>>>>
                            holdingTestRunExplorationState:
                        {
                            var (testRun, remainingTestRunExploration) =
                                holdingTestRunExplorationState.RemainingTestRunExploration;

                            var nextState = new MaybeHoldingTestRunState<Property.Test<T>, IEnumerable<ExplorationStage<Property.Test<T>>>>(
                                testRun,
                                remainingTestRunExploration,
                                holdingTestRunExplorationState.TestRunExplorationStateData);

                            stateMachine.Jump(nextState);
                            break;
                        }
                        default:
                            throw new NotImplementedException($"Unrecognized state: {stateMachine.State}");
                    }
                }
                else
                {
                    stateMachine.Transition();
                }
            }

            return stateMachine.Result;
        }


        public static async Task<CheckResult<T>> CheckAsync<T>(
            this IGen<Property.AsyncTest<T>> property,
            int? iterations = null,
            int? seed = null,
            int? size = null,
            int? shrinkLimit = null,
            string? replay = null,
            bool deepCheck = true)
        {
            var stateMachineDriver = new AsyncCheckStateMachineDriver<T>();
            var stateMachine = CreateStateMachine<T, Property.AsyncTest<T>, IAsyncEnumerable<ExplorationStage<Property.AsyncTest<T>>>>(
                stateMachineDriver, property, iterations, seed, size, shrinkLimit, replay, deepCheck);

            while (stateMachine.State is not TerminalState<Property.AsyncTest<T>>)
            {
                if (stateMachine.State is InterruptedCheckState<Property.AsyncTest<T>>)
                {
                    switch (stateMachine.State)
                    {
                        case HoldingInstanceState<Property.AsyncTest<T>> holdingInstanceState:
                        {
                            var instance = holdingInstanceState.Instance;

                            var initialTestRunExploration = instance.ExampleSpace.ExploreAsync(CheckTestAsync<T>());

                            var nextState =
                                new BeginTestRunExplorationState<Property.AsyncTest<T>, IAsyncEnumerable<ExplorationStage<Property.AsyncTest<T>>>>(
                                    instance,
                                    initialTestRunExploration);

                            stateMachine.Jump(nextState);
                            break;
                        }
                        case HoldingTestRunExplorationState<Property.AsyncTest<T>, IAsyncEnumerable<ExplorationStage<Property.AsyncTest<T>>>>
                            holdingTestRunExplorationState:
                        {
                            var (testRun, remainingTestRunExploration) =
                                await holdingTestRunExplorationState.RemainingTestRunExploration.Deconstruct();

                            var nextState =
                                new MaybeHoldingTestRunState<Property.AsyncTest<T>, IAsyncEnumerable<ExplorationStage<Property.AsyncTest<T>>>>(
                                    testRun,
                                    remainingTestRunExploration,
                                    holdingTestRunExplorationState.TestRunExplorationStateData);

                            stateMachine.Jump(nextState);
                            break;
                        }
                        default:
                            throw new NotImplementedException($"Unrecognized state: {stateMachine.State}");
                    }
                }
                else
                {
                    stateMachine.Transition();
                }
            }

            return stateMachine.Result;
        }

        private static CheckStateMachine<T, Test, TestRunExploration> CreateStateMachine<T, Test, TestRunExploration>(
            ICheckStateMachineDriver<T, Test> driver,
            IGen<Test> property,
            int? iterations,
            int? seed,
            int? size,
            int? shrinkLimit,
            string? replay,
            bool deepCheck
        )
            where Test : Property.TestInput<T>
        {
            var resolvedIterations = iterations ?? 100;

            var (initialSize, resizeStrategy) =
                SizingAspects<Test>.Resolve(size == null ? null : new Size(size.Value),
                    resolvedIterations);

            var initialParameters = seed == null
                ? GenParameters.CreateRandom(initialSize)
                : GenParameters.Create(Rng.Create(seed.Value), initialSize);

            TransitionableCheckState<Test> initialState = replay is null
                ? new InitialState<Test>()
                : new ReplayState<Test>(replay);

            var initialStateData =
                CheckStateData<Test>.Initial(
                    property: property,
                    resizeStrategy: resizeStrategy,
                    requestedIterations: resolvedIterations,
                    shrinkLimit: shrinkLimit ?? 500,
                    deepCheck: deepCheck,
                    isReplay: replay is not null,
                    initialParameters: initialParameters);

            return new CheckStateMachine<T, Test, TestRunExploration>(driver, initialState, initialStateData);
        }

        private static AnalyzeExploration<Property.Test<T>> CheckTest<T>() => example =>
        {
            var testOutput = example.Value.Output.Value;
            return testOutput.Result switch
            {
                Property.TestResult.Succeeded => ExplorationOutcome.Success(),
                Property.TestResult.Failed => ExplorationOutcome.Fail(testOutput.Exception),
                _ => throw new Exception("Fatal: Unhandled case")
            };
        };

        private static AnalyzeExplorationAsync<Property.AsyncTest<T>> CheckTestAsync<T>() => async example =>
        {
            var testOutput = await example.Value.Output.Value;
            return testOutput.Result switch
            {
                Property.TestResult.Succeeded => ExplorationOutcome.Success(),
                Property.TestResult.Failed => ExplorationOutcome.Fail(testOutput.Exception),
                _ => throw new Exception("Fatal: Unhandled case")
            };
        };
    }
}
