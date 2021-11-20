using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Internal;
using GalaxyCheck.Runners.Check;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal static class GenerationStates
    {
        internal record Generation_Begin<T>(CheckStateContext<T> Context) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState()
            {
                if (Context.CompletedIterations >= Context.RequestedIterations)
                {
                    return new TerminationState<T>(Context, TerminationReason.ReachedMaximumIterations);
                }

                var iterations = Context.Property.Advanced.Run(Context.NextParameters);
                return new Generation_HoldingNextIteration<T>(Context, iterations);
            }
        }

        internal record Generation_HoldingNextIteration<T>(
            CheckStateContext<T> Context,
            IEnumerable<IGenIteration<Test<T>>> Iterations) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState()
            {
                var (head, tail) = Iterations;
                return head!.Match<AbstractCheckState<T>>(
                    onInstance: (instance) =>
                        new Generation_Instance<T>(Context, instance),
                    onDiscard: (discard) =>
                        new Generation_Discard<T>(Context, tail, discard),
                    onError: (error) =>
                        new Generation_Error<T>(Context, $"Error while running generator {error.GenName}: {error.Message}"));
            }
        }

        internal record Generation_Instance<T>(
            CheckStateContext<T> Context,
            IGenInstance<Test<T>> Iteration) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState() => new InstanceExplorationStates.InstanceExploration_Begin<T>(Context, Iteration);
        }

        internal record Generation_Discard<T>(
            CheckStateContext<T> Context,
            IEnumerable<IGenIteration<Test<T>>> NextIterations,
            IGenDiscard<Test<T>> Iteration) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState() => new Generation_HoldingNextIteration<T>(Context.IncrementDiscards(), NextIterations);
        }

        internal record Generation_Error<T>(
            CheckStateContext<T> State,
            string Description) : AbstractCheckState<T>(State)
        {
            internal override AbstractCheckState<T> NextState() => new TerminationState<T>(State, TerminationReason.FoundError);
        }

        internal record Generation_End<T>(
            CheckStateContext<T> Context,
            IGenInstance<Test<T>> Instance,
            CounterexampleContext<T>? CounterexampleContext,
            bool WasDiscard,
            bool WasReplay) : AbstractCheckState<T>(Context)
        {
            internal override AbstractCheckState<T> NextState() => WasDiscard == true
                ? NextStateOnDiscard(Context)
                : CounterexampleContext == null
                    ? NextStateWithoutCounterexample(Context, Instance)
                    : NextStateWithCounterexample(Context, Instance, CounterexampleContext, WasReplay);

            private static AbstractCheckState<T> NextStateOnDiscard(CheckStateContext<T> state)
            {
                return new Generation_Begin<T>(state.IncrementDiscards());
            }

            private static AbstractCheckState<T> NextStateWithoutCounterexample(
                CheckStateContext<T> state,
                IGenInstance<Test<T>> instance)
            {
                var nextSize = Resize(state, null, instance);
                return new Generation_Begin<T>(state
                    .IncrementCompletedIterations()
                    .WithNextGenParameters(instance.NextParameters.With(size: nextSize)));
            }

            private static AbstractCheckState<T> NextStateWithCounterexample(
                CheckStateContext<T> state,
                IGenInstance<Test<T>> instance,
                CounterexampleContext<T> counterexampleContext,
                bool wasReplay)
            {
                var nextSize = Resize(state, counterexampleContext, instance);

                var nextState = state
                    .IncrementCompletedIterations()
                    .WithNextGenParameters(instance.NextParameters.With(size: nextSize))
                    .AddCounterexample(counterexampleContext);

                var terminationReason = TryTerminate(nextState, counterexampleContext, instance, wasReplay);

                return terminationReason == null
                    ? new Generation_Begin<T>(nextState.WithNextGenParameters(instance.NextParameters.With(size: nextSize)))
                    : new TerminationState<T>(nextState, terminationReason.Value);
            }

            private static Size Resize(
                CheckStateContext<T> state,
                CounterexampleContext<T>? counterexampleContext,
                IGenIteration<Test<T>> instance) =>
                    state.ResizeStrategy(new ResizeStrategyInformation<T>(state, counterexampleContext, instance));

            private static TerminationReason? TryTerminate(
                CheckStateContext<T> state,
                CounterexampleContext<T> counterexampleContext,
                IGenInstance<Test<T>> instance,
                bool wasReplay)
            {
                if (wasReplay)
                {
                    // We just want to repeat the last failure. The check we are replaying presumably reached for the
                    // smallest counterexample anyway.
                    return TerminationReason.IsReplay;
                }

                if (!state.DeepCheck)
                {
                    return TerminationReason.DeepCheckDisabled;
                }

                if (state.CompletedIterations >= state.RequestedIterations)
                {
                    // We should stop reaching for smaller counterexamples if we are at max iterations already.
                    return TerminationReason.ReachedMaximumIterations;
                }

                if (state.Shrinks >= state.ShrinkLimit)
                {
                    return TerminationReason.ReachedShrinkLimit;
                }

                if (instance.ReplayParameters.Size.Value == 100)
                {
                    // We're at the max size, don't bother starting again from 0% as the compute time is most likely not
                    // worth it. TODO: Add config to disable this if someone REALLY wants to find a smaller counterexample.
                    return TerminationReason.ReachedMaximumSize;
                }

                if (counterexampleContext.ExampleSpace.Current.Distance == 0)
                {
                    // The counterexample is literally the smallest possible example that fits the constraints.
                    return TerminationReason.FoundTheoreticalSmallestCounterexample;
                }

                const int RecentCounterexampleThreshold = 3;
                var recentCounterexamples = state.CounterexampleContextHistory.Take(RecentCounterexampleThreshold).ToList();
                if (recentCounterexamples.Count() == RecentCounterexampleThreshold &&
                    recentCounterexamples.GroupBy(x => x.ExampleSpace.Current.Id).Count() == 1)
                {
                    // The recent counterexamples have settled somewhat, short-circuit the remaining iterations
                    return TerminationReason.FoundPragmaticSmallestCounterexample;
                }

                return null;
            }
        }
    }
}
