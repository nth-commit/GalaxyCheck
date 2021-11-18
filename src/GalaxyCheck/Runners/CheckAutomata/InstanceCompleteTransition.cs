using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Runners.Check;
using System.Linq;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal record InstanceCompleteTransition<T>(
        CheckState<T> State,
        IGenInstance<Test<T>> Instance,
        CounterexampleState<T>? CounterexampleState,
        bool WasDiscard,
        bool WasReplay) : AbstractTransition<T>(State)
    {
        internal override AbstractTransition<T> NextTransition() => WasDiscard == true
            ? NextStateOnDiscard(State, Instance)
            : CounterexampleState == null
                ? NextStateWithoutCounterexample(State, Instance)
                : NextStateWithCounterexample(State, Instance, CounterexampleState, WasReplay);

        private static AbstractTransition<T> NextStateOnDiscard(
            CheckState<T> state,
            IGenInstance<Test<T>> instance)
        {
            var nextSize = Resize(state, null, instance);
            // TODO: Resize on discards more like Gen.Where does
            return new InitialTransition<T>(state
                .IncrementDiscards()
                .WithNextGenParameters(instance.NextParameters.With(size: nextSize)));
        }

        private static AbstractTransition<T> NextStateWithoutCounterexample(
            CheckState<T> state,
            IGenInstance<Test<T>> instance)
        {
            var nextSize = Resize(state, null, instance);
            return new InitialTransition<T>(state
                .IncrementCompletedIterations()
                .WithNextGenParameters(instance.NextParameters.With(size: nextSize)));
        }

        private static AbstractTransition<T> NextStateWithCounterexample(
            CheckState<T> state,
            IGenInstance<Test<T>> instance,
            CounterexampleState<T> counterexampleState,
            bool wasReplay)
        {
            var nextSize = Resize(state, counterexampleState, instance);

            var nextState = state
                .IncrementCompletedIterations()
                .WithNextGenParameters(instance.NextParameters.With(size: nextSize))
                .AddCounterexample(counterexampleState);

            var terminationReason = TryTerminate(nextState, counterexampleState, instance, wasReplay);

            return terminationReason == null
                ? new InitialTransition<T>(nextState.WithNextGenParameters(instance.NextParameters.With(size: nextSize)))
                : new Termination<T>(nextState, terminationReason.Value);
        }

        private static Size Resize(
            CheckState<T> state,
            CounterexampleState<T>? counterexampleState,
            IGenIteration<Test<T>> instance) =>
                state.ResizeStrategy(new ResizeStrategyInformation<T>(state, counterexampleState, instance));

        private static TerminationReason? TryTerminate(
            CheckState<T> state,
            CounterexampleState<T> counterexampleState,
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
                
            if (counterexampleState.ExampleSpace.Current.Distance == 0)
            {
                // The counterexample is literally the smallest possible example that fits the constraints.
                return TerminationReason.FoundTheoreticalSmallestCounterexample;
            }

            const int RecentCounterexampleThreshold = 3;
            var recentCounterexamples = state.CounterexampleStateHistory.Take(RecentCounterexampleThreshold).ToList();
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
