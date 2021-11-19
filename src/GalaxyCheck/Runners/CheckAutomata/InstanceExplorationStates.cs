using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal static class InstanceExplorationStates
    {
        internal record Begin<T>(
            CheckStateContext<T> Context,
            IGenInstance<Test<T>> Instance) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState()
            {
                var explorations = Instance.ExampleSpace.Explore(AnalyzeExplorationForCheck.Impl<T>());

                return new HoldingNextExplorationStage<T>(Context, Instance, explorations, null, true);
            }
        }

        internal record HoldingNextExplorationStage<T>(
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
                    return new GenerationStates.End<T>(
                        Context,
                        Instance,
                        CounterexampleContext,
                        WasDiscard: false,
                        WasReplay: false);
                }

                if (IsFirstExplorationStage == false && Context.Shrinks >= Context.ShrinkLimit)
                {
                    return new GenerationStates.End<T>(
                        Context,
                        Instance,
                        CounterexampleContext,
                        WasDiscard: false,
                        WasReplay: false);
                }

                var ctx = IsFirstExplorationStage ? Context : Context.IncrementShrinks();

                return head.Match<AbstractCheckState<T>>(
                    onCounterexampleExploration: (counterexampleExploration) =>
                        new Counterexample<T>(
                            ctx,
                            Instance,
                            tail,
                            counterexampleExploration),

                    onNonCounterexampleExploration: (nonCounterexampleExploration) =>
                        new NonCounterexample<T>(
                            ctx,
                            Instance,
                            tail,
                            nonCounterexampleExploration,
                            CounterexampleContext),

                    onDiscardExploration: (_) => IsFirstExplorationStage
                        ? new GenerationStates.End<T>(ctx, Instance, CounterexampleContext: null, WasDiscard: true, WasReplay: false)
                        : new Discard<T>(ctx, Instance, tail, CounterexampleContext));
            }
        }

        internal record Counterexample<T>(
            CheckStateContext<T> Context,
            IGenInstance<Test<T>> Instance,
            IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
            ExplorationStage<Test<T>>.Counterexample CounterexampleExploration) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState() => new HoldingNextExplorationStage<T>(
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

        internal record NonCounterexample<T>(
            CheckStateContext<T> Context,
            IGenInstance<Test<T>> Instance,
            IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
            ExplorationStage<Test<T>>.NonCounterexample NonCounterexampleExploration,
            CounterexampleContext<T>? PreviousCounterexampleContext) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState() => new HoldingNextExplorationStage<T>(
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

        internal record Discard<T>(
            CheckStateContext<T> Context,
            IGenInstance<Test<T>> Instance,
            IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
            CounterexampleContext<T>? PreviousCounterexample) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState() => new HoldingNextExplorationStage<T>(
                Context,
                Instance,
                NextExplorations,
                PreviousCounterexample,
                IsFirstExplorationStage: false);
        }
    }
}
