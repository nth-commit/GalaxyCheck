using GalaxyCheck;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalaxyCheck.Runners.Check.AsyncAutomata
{
    internal static class InstanceExplorationStates
    {
        internal record InstanceExploration_Begin<T>(IGenInstance<Property.AsyncTest<T>> Instance) : CheckState<T>
        {
            public Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context)
            {
                var explorations = Instance.ExampleSpace.ExploreAsync(AnalyzeExplorationForCheck.CheckTestAsync<T>());

                return Task.FromResult(new CheckStateTransition<T>(
                    new InstanceExploration_HoldingNextExplorationStage<T>(Instance, explorations, null, true),
                    context));
            }
        }

        internal record InstanceExploration_HoldingNextExplorationStage<T>(
            IGenInstance<Property.AsyncTest<T>> Instance,
            IAsyncEnumerable<ExplorationStage<Property.AsyncTest<T>>> Explorations,
            CounterexampleContext<T>? CounterexampleContext,
            bool IsFirstExplorationStage) : CheckState<T>
        {
            public async Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context)
            {
                var (head, tail) = await Explorations.Deconstruct();

                if (head == null)
                {
                    return new CheckStateTransition<T>(
                        new InstanceExploration_End<T>(Instance, CounterexampleContext, WasDiscard: false, WasReplay: false),
                        context);
                }

                if (IsFirstExplorationStage == false && context.Shrinks >= context.ShrinkLimit)
                {
                    return new CheckStateTransition<T>(
                        new InstanceExploration_End<T>(Instance, CounterexampleContext, WasDiscard: false, WasReplay: false),
                        context);
                }

                var nextContext = IsFirstExplorationStage ? context : context.IncrementShrinks();

                var nextState = head.Match<CheckState<T>>(
                    onCounterexampleExploration: (counterexampleExploration) =>
                        new InstanceExploration_Counterexample<T>(Instance, tail, counterexampleExploration),

                    onNonCounterexampleExploration: (nonCounterexampleExploration) =>
                        new InstanceExploration_NonCounterexample<T>(Instance, tail, nonCounterexampleExploration, CounterexampleContext));

                return new CheckStateTransition<T>(nextState, nextContext);
            }
        }

        internal record InstanceExploration_Counterexample<T>(
            IGenInstance<Property.AsyncTest<T>> Instance,
            IAsyncEnumerable<ExplorationStage<Property.AsyncTest<T>>> NextExplorations,
            ExplorationStage<Property.AsyncTest<T>>.Counterexample CounterexampleExploration) : CheckState<T>
        {
            public Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context) => Task.FromResult(
                new CheckStateTransition<T>(
                    new InstanceExploration_HoldingNextExplorationStage<T>(
                        Instance,
                        NextExplorations,
                        CounterexampleContext,
                        IsFirstExplorationStage: false),
                    context));

            public CounterexampleContext<T> CounterexampleContext => new CounterexampleContext<T>(
                CounterexampleExploration.ExampleSpace,
                Instance.ReplayParameters,
                CounterexampleExploration.Path,
                CounterexampleExploration.Exception);

            public IExampleSpace<Property.AsyncTest<T>> TestExampleSpace => CounterexampleExploration.ExampleSpace;

            public IExampleSpace<T> InputExampleSpace => CounterexampleExploration.ExampleSpace.Map(ex => ex.Input);

            public IEnumerable<int> Path => CounterexampleExploration.Path;
        }

        internal record InstanceExploration_NonCounterexample<T>(
            IGenInstance<Property.AsyncTest<T>> Instance,
            IAsyncEnumerable<ExplorationStage<Property.AsyncTest<T>>> NextExplorations,
            ExplorationStage<Property.AsyncTest<T>>.NonCounterexample NonCounterexampleExploration,
            CounterexampleContext<T>? PreviousCounterexampleContext) : CheckState<T>
        {
            public Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context) => Task.FromResult(
                new CheckStateTransition<T>(
                    new InstanceExploration_HoldingNextExplorationStage<T>(
                        Instance,
                        NextExplorations,
                        PreviousCounterexampleContext,
                        IsFirstExplorationStage: false),
                    context));

            public IExampleSpace<Property.AsyncTest<T>> TestExampleSpace => NonCounterexampleExploration.ExampleSpace;

            public IExampleSpace<T> InputExampleSpace => NonCounterexampleExploration.ExampleSpace.Map(ex => ex.Input);

            public IEnumerable<int> Path => NonCounterexampleExploration.Path;
        }

        internal record InstanceExploration_End<T>(
            IGenInstance<Property.AsyncTest<T>> Instance,
            CounterexampleContext<T>? CounterexampleContext,
            bool WasDiscard,
            bool WasReplay) : CheckState<T>
        {
            public Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context) => Task.FromResult(
                new CheckStateTransition<T>(
                    new GenerationStates.Generation_End<T>(Instance, CounterexampleContext, WasDiscard, WasDiscard, WasReplay),
                    context));
        }
    }
}
