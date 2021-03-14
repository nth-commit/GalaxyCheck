using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Sizing;
using System.Linq;
using static GalaxyCheck.CheckExtensions;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public record InstanceCompleteTransition<T>(
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
            CheckState<T> context,
            IGenInstance<Test<T>> instance)
        {
            // TODO: Resize on discards more like Gen.Where does
            // TODO: Exhaustion protection
            var nextSize = context.ResizeStrategy(instance, wasCounterexample: false);
            return new InitialTransition<T>(context
                .IncrementDiscards()
                .WithNextGenParameters(instance.NextParameters.With(size: nextSize)));
        }

        private static AbstractTransition<T> NextStateWithoutCounterexample(
            CheckState<T> context,
            IGenInstance<Test<T>> instance)
        {
            var nextSize = context.ResizeStrategy(instance, wasCounterexample: false);
            return new InitialTransition<T>(context
                .IncrementCompletedIterations()
                .WithNextGenParameters(instance.NextParameters.With(size: nextSize)));
        }

        private static AbstractTransition<T> NextStateWithCounterexample(
            CheckState<T> context,
            IGenInstance<Test<T>> instance,
            CounterexampleState<T> counterexampleState,
            bool wasReplay)
        {
            var nextSize = context.ResizeStrategy(instance, wasCounterexample: true);
            var nextState = context
                .IncrementCompletedIterations()
                .WithNextGenParameters(instance.NextParameters.With(size: nextSize))
                .AddCounterexample(counterexampleState);

            if (wasReplay)
            {
                return new Termination<T>(nextState);
            }

            if (nextState.CompletedIterations == nextState.RequestedIterations)
            {
                return new Termination<T>(nextState);
            }

            if (counterexampleState.ExampleSpace.Current.Distance == 0)
            {
                // The counterexample is literally the smallest possible example. Can't get smaller than that.
                return new Termination<T>(nextState);
            }

            if (instance.RepeatParameters.Size.Value == 100)
            {
                // We're at the max size, don't bother starting again from 0% as the compute time is most
                // likely not worth it. TODO: Add config to disable this if someone REALLY wants to find a
                // smaller counterexample.
                return new Termination<T>(nextState);
            }

            const int RecentCounterexampleThreshold = 3;
            var recentCounterexamples = nextState.CounterexampleStateHistory.Take(RecentCounterexampleThreshold).ToList();
            if (recentCounterexamples.Count() == RecentCounterexampleThreshold &&
                recentCounterexamples.GroupBy(x => x.ExampleSpace.Current.Id).Count() == 1)
            {
                // The recent counterexamples have settled somewhat, short-circuit the remaining iterations
                return new Termination<T>(nextState);
            }

            return new InitialTransition<T>(nextState.WithNextGenParameters(instance.NextParameters.With(size: nextSize)));
        }
    }
}
