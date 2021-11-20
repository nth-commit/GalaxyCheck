using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal static class InstanceExplorationStates
    {
        internal record InstanceExploration_Begin<T>(IGenInstance<Test<T>> Instance) : CheckState<T>
        {
            public CheckStateTransition<T> Transition(CheckStateContext<T> context)
            {
                var explorations = Instance.ExampleSpace.Explore(AnalyzeExplorationForCheck.Impl<T>());
                return new CheckStateTransition<T>(
                    new InstanceExploration_HoldingNextExplorationStage<T>(Instance, explorations, null, true),
                    context);
            }
        }

        internal record InstanceExploration_HoldingNextExplorationStage<T>(
            IGenInstance<Test<T>> Instance,
            IEnumerable<ExplorationStage<Test<T>>> Explorations,
            CounterexampleContext<T>? CounterexampleContext,
            bool IsFirstExplorationStage) : CheckState<T>
        {
            public CheckStateTransition<T> Transition(CheckStateContext<T> context)
            {
                var (head, tail) = Explorations;

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
            IGenInstance<Test<T>> Instance,
            IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
            ExplorationStage<Test<T>>.Counterexample CounterexampleExploration) : CheckState<T>
        {
            public CheckStateTransition<T> Transition(CheckStateContext<T> context) => new CheckStateTransition<T>(
                new InstanceExploration_HoldingNextExplorationStage<T>(
                    Instance,
                    NextExplorations,
                    CounterexampleContext,
                    IsFirstExplorationStage: false),
                context);

            public CounterexampleContext<T> CounterexampleContext => new CounterexampleContext<T>(
                CounterexampleExploration.ExampleSpace,
                ExampleSpaceHistory,
                Instance.ReplayParameters,
                CounterexampleExploration.Path,
                CounterexampleExploration.Exception);

            public IExampleSpace<Test<T>> TestExampleSpace => CounterexampleExploration.ExampleSpace;

            public IExampleSpace<T> InputExampleSpace => CounterexampleExploration.ExampleSpace.Map(ex => ex.Input);

            public IEnumerable<int> Path => CounterexampleExploration.Path;

            public IEnumerable<Lazy<IExampleSpace<object>?>> ExampleSpaceHistory => Instance.ExampleSpaceHistory.Select(
                exs => new Lazy<IExampleSpace<object>?>(() => exs.Cast<object>().Navigate(Path)));
        }

        internal record InstanceExploration_NonCounterexample<T>(
            IGenInstance<Test<T>> Instance,
            IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
            ExplorationStage<Test<T>>.NonCounterexample NonCounterexampleExploration,
            CounterexampleContext<T>? PreviousCounterexampleContext) : CheckState<T>
        {
            public CheckStateTransition<T> Transition(CheckStateContext<T> context) => new CheckStateTransition<T>(
                new InstanceExploration_HoldingNextExplorationStage<T>(
                    Instance,
                    NextExplorations,
                    PreviousCounterexampleContext,
                    IsFirstExplorationStage: false),
                context);

            public IExampleSpace<Test<T>> TestExampleSpace => NonCounterexampleExploration.ExampleSpace;

            public IExampleSpace<T> InputExampleSpace => NonCounterexampleExploration.ExampleSpace.Map(ex => ex.Input);

            public IEnumerable<int> Path => NonCounterexampleExploration.Path;

            public IEnumerable<Lazy<IExampleSpace<object>?>> ExampleSpaceHistory => Instance.ExampleSpaceHistory.Select(
                exs => new Lazy<IExampleSpace<object>?>(() => exs.Cast<object>().Navigate(Path)));
        }

        internal record InstanceExploration_Discard<T>(
            IGenInstance<Test<T>> Instance,
            IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
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
            public CheckStateTransition<T> Transition(CheckStateContext<T> context)
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

                return new CheckStateTransition<T>(nextState, context);
            }
        }

        internal record InstanceExploration_End<T>(
            IGenInstance<Test<T>> Instance,
            CounterexampleContext<T>? CounterexampleContext,
            bool WasDiscard,
            bool WasReplay) : CheckState<T>
        {
            public CheckStateTransition<T> Transition(CheckStateContext<T> context) => new CheckStateTransition<T>(
                new GenerationStates.Generation_End<T>(Instance, CounterexampleContext, WasDiscard, WasReplay),
                context);
        }
    }
}
