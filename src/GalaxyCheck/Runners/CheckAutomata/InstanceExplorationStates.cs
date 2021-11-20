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
        internal record InstanceExploration_Begin<T>(
            CheckStateContext<T> Context,
            IGenInstance<Test<T>> Instance) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState()
            {
                var explorations = Instance.ExampleSpace.Explore(AnalyzeExplorationForCheck.Impl<T>());

                return new InstanceExploration_HoldingNextExplorationStage<T>(Context, Instance, explorations, null, true);
            }
        }

        internal record InstanceExploration_HoldingNextExplorationStage<T>(
            CheckStateContext<T> Context,
            IGenInstance<Test<T>> Instance,
            IEnumerable<ExplorationStage<Test<T>>> Explorations,
            CounterexampleContext<T>? CounterexampleContext,
            bool IsFirstExplorationStage) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState()
            {
                var (head, tail) = Explorations;

                if (head == null)
                {
                    return new InstanceExploration_End<T>(
                        Context,
                        Instance,
                        CounterexampleContext,
                        WasDiscard: false,
                        WasReplay: false);
                }

                if (IsFirstExplorationStage == false && Context.Shrinks >= Context.ShrinkLimit)
                {
                    return new InstanceExploration_End<T>(
                        Context,
                        Instance,
                        CounterexampleContext,
                        WasDiscard: false,
                        WasReplay: false);
                }

                var ctx = IsFirstExplorationStage ? Context : Context.IncrementShrinks();

                return head.Match<AbstractCheckState<T>>(
                    onCounterexampleExploration: (counterexampleExploration) =>
                        new InstanceExploration_Counterexample<T>(
                            ctx,
                            Instance,
                            tail,
                            counterexampleExploration),

                    onNonCounterexampleExploration: (nonCounterexampleExploration) =>
                        new InstanceExploration_NonCounterexample<T>(
                            ctx,
                            Instance,
                            tail,
                            nonCounterexampleExploration,
                            CounterexampleContext),

                    onDiscardExploration: (_) => new InstanceExploration_Discard<T>(ctx, Instance, tail, CounterexampleContext, IsFirstExplorationStage));
            }
        }

        internal record InstanceExploration_Counterexample<T>(
            CheckStateContext<T> Context,
            IGenInstance<Test<T>> Instance,
            IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
            ExplorationStage<Test<T>>.Counterexample CounterexampleExploration) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState() => new InstanceExploration_HoldingNextExplorationStage<T>(
                Context,
                Instance,
                NextExplorations,
                CounterexampleContext,
                IsFirstExplorationStage: false);

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
            CheckStateContext<T> Context,
            IGenInstance<Test<T>> Instance,
            IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
            ExplorationStage<Test<T>>.NonCounterexample NonCounterexampleExploration,
            CounterexampleContext<T>? PreviousCounterexampleContext) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState() => new InstanceExploration_HoldingNextExplorationStage<T>(
                Context,
                Instance,
                NextExplorations,
                PreviousCounterexampleContext,
                IsFirstExplorationStage: false);

            public IExampleSpace<Test<T>> TestExampleSpace => NonCounterexampleExploration.ExampleSpace;

            public IExampleSpace<T> InputExampleSpace => NonCounterexampleExploration.ExampleSpace.Map(ex => ex.Input);

            public IEnumerable<int> Path => NonCounterexampleExploration.Path;

            public IEnumerable<Lazy<IExampleSpace<object>?>> ExampleSpaceHistory => Instance.ExampleSpaceHistory.Select(
                exs => new Lazy<IExampleSpace<object>?>(() => exs.Cast<object>().Navigate(Path)));
        }

        internal record InstanceExploration_Discard<T>(
            CheckStateContext<T> Context,
            IGenInstance<Test<T>> Instance,
            IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
            CounterexampleContext<T>? PreviousCounterexample,
            bool IsFirstExplorationStage) : AbstractCheckState<T>(Context)
        {
            /// <summary>
            /// It's somewhat surprising that this state can be the first exploration (the top of the tree and a
            /// discard). If it was the first exploration, you would think the discard would be handled by
            /// GenerationStates.Discard. However, this code path is hit by "late filtering"
            /// (<see cref="Property.Precondition(bool)"/>). We don't find out this is a discard until we start
            /// exploring the example space.
            /// </summary>
            internal override AbstractCheckState<T> NextState() => IsFirstExplorationStage
                ? new InstanceExploration_End<T>(Context, Instance, CounterexampleContext: null, WasDiscard: true, WasReplay: false)
                : new InstanceExploration_HoldingNextExplorationStage<T>(Context, Instance, NextExplorations, PreviousCounterexample, IsFirstExplorationStage: false);
        }

        internal record InstanceExploration_End<T>(
            CheckStateContext<T> Context,
            IGenInstance<Test<T>> Instance,
            CounterexampleContext<T>? CounterexampleContext,
            bool WasDiscard,
            bool WasReplay) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState() => new GenerationStates.Generation_End<T>(
                Context,
                Instance,
                CounterexampleContext,
                WasDiscard,
                WasReplay);
        }
    }
}
