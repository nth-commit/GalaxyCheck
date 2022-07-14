using GalaxyCheck;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalaxyCheck.Runners.Check.AutomataAsync
{
    internal static class InstanceExplorationStates
    {
        internal record InstanceExploration_Begin<T>(IGenInstance<TestAsync<T>> Instance) : CheckState<T>
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
            IGenInstance<TestAsync<T>> Instance,
            IAsyncEnumerable<ExplorationStage<TestAsync<T>>> Explorations,
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
                        new InstanceExploration_NonCounterexample<T>(Instance, tail, nonCounterexampleExploration, CounterexampleContext),

                    onDiscardExploration: (_) =>
                        new InstanceExploration_Discard<T>(Instance, tail, CounterexampleContext, IsFirstExplorationStage));

                return new CheckStateTransition<T>(nextState, nextContext);
            }
        }

        internal record InstanceExploration_Counterexample<T>(
            IGenInstance<TestAsync<T>> Instance,
            IAsyncEnumerable<ExplorationStage<TestAsync<T>>> NextExplorations,
            ExplorationStage<TestAsync<T>>.Counterexample CounterexampleExploration) : CheckState<T>
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
                ExampleSpaceHistory,
                Instance.ReplayParameters,
                CounterexampleExploration.Path,
                CounterexampleExploration.Exception);

            public IExampleSpace<TestAsync<T>> TestExampleSpace => CounterexampleExploration.ExampleSpace;

            public IExampleSpace<T> InputExampleSpace => CounterexampleExploration.ExampleSpace.Map(ex => ex.Input);

            public IEnumerable<int> Path => CounterexampleExploration.Path;

            public IEnumerable<Lazy<IExampleSpace<object>?>> ExampleSpaceHistory => Instance.ExampleSpaceHistory.Select(
                exs => new Lazy<IExampleSpace<object>?>(() => exs.Cast<object>().Navigate(Path)));
        }

        internal record InstanceExploration_NonCounterexample<T>(
            IGenInstance<TestAsync<T>> Instance,
            IAsyncEnumerable<ExplorationStage<TestAsync<T>>> NextExplorations,
            ExplorationStage<TestAsync<T>>.NonCounterexample NonCounterexampleExploration,
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

            public IExampleSpace<TestAsync<T>> TestExampleSpace => NonCounterexampleExploration.ExampleSpace;

            public IExampleSpace<T> InputExampleSpace => NonCounterexampleExploration.ExampleSpace.Map(ex => ex.Input);

            public IEnumerable<int> Path => NonCounterexampleExploration.Path;

            public IEnumerable<Lazy<IExampleSpace<object>?>> ExampleSpaceHistory => Instance.ExampleSpaceHistory.Select(
                exs => new Lazy<IExampleSpace<object>?>(() => exs.Cast<object>().Navigate(Path)));
        }

        internal record InstanceExploration_Discard<T>(
            IGenInstance<TestAsync<T>> Instance,
            IAsyncEnumerable<ExplorationStage<TestAsync<T>>> NextExplorations,
            CounterexampleContext<T>? PreviousCounterexample,
            bool IsFirstExplorationStage) : CheckState<T>
        {
            /// <summary>
            /// It's somewhat surprising that this state can be the first exploration (the top of the tree and a
            /// discard). If it was the first exploration, you would think the discard would be handled by
            /// GenerationStates.Discard. However, this code path is hit by "late filtering"
            /// (<see cref="Property.Precondition(bool)"/>). We don't find out this is a discard until we start
            /// exploring the example space.
            /// </summary>
            public Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context)
            {
                CheckState<T> nextState = IsFirstExplorationStage
                    ? new InstanceExploration_End<T>(
                        Instance,
                        CounterexampleContext: null,
                        WasDiscard: true,
                        WasReplay: false)
                    : new InstanceExploration_HoldingNextExplorationStage<T>(
                        Instance,
                        NextExplorations,
                        PreviousCounterexample,
                        IsFirstExplorationStage: false);

                return Task.FromResult(new CheckStateTransition<T>(nextState, context));
            }
        }

        internal record InstanceExploration_End<T>(
            IGenInstance<TestAsync<T>> Instance,
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
