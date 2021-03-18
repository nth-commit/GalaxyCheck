using System.Collections.Immutable;
using System.Linq;
using static GalaxyCheck.CheckExtensions;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public record CheckState<T>(
        Property<T> Property,
        int RequestedIterations,
        int ShrinkLimit,
        int CompletedIterations,
        int Discards,
        int Shrinks,
        ImmutableList<CounterexampleState<T>> CounterexampleStateHistory,
        GenParameters NextParameters,
        ResizeStrategy<T> ResizeStrategy,
        bool DeepCheck)
    {
        internal static CheckState<T> Create(
            Property<T> property,
            int requestedIterations,
            int shrinkLimit,
            GenParameters initialParameters,
            ResizeStrategy<T> resizeStrategy,
            bool deepCheck) =>
                new CheckState<T>(
                    property,
                    requestedIterations,
                    shrinkLimit,
                    0,
                    0,
                    0,
                    ImmutableList.Create<CounterexampleState<T>>(),
                    initialParameters,
                    resizeStrategy,
                    deepCheck);

        public CounterexampleState<T>? Counterexample => CounterexampleStateHistory
            .OrderBy(c => c.ExampleSpace.Current.Distance)
            .ThenByDescending(c => c.ReplayParameters.Size.Value) // Bigger sizes are more likely to normalize
            .FirstOrDefault();

        internal CheckState<T> IncrementCompletedIterations() => new CheckState<T>(
            Property,
            RequestedIterations,
            ShrinkLimit,
            CompletedIterations + 1,
            Discards,
            Shrinks,
            CounterexampleStateHistory,
            NextParameters,
            ResizeStrategy,
            DeepCheck);

        internal CheckState<T> IncrementDiscards() => new CheckState<T>(
            Property,
            RequestedIterations,
            ShrinkLimit,
            CompletedIterations,
            Discards + 1,
            Shrinks,
            CounterexampleStateHistory,
            NextParameters,
            ResizeStrategy,
            DeepCheck);

        internal CheckState<T> IncrementShrinks() => new CheckState<T>(
            Property,
            RequestedIterations,
            ShrinkLimit,
            CompletedIterations,
            Discards,
            Shrinks + 1,
            CounterexampleStateHistory,
            NextParameters,
            ResizeStrategy,
            DeepCheck);

        internal CheckState<T> AddCounterexample(CounterexampleState<T> counterexampleState) => new CheckState<T>(
            Property,
            RequestedIterations,
            ShrinkLimit,
            CompletedIterations,
            Discards,
            Shrinks,
            Enumerable.Concat(new[] { counterexampleState }, CounterexampleStateHistory).ToImmutableList(),
            NextParameters,
            ResizeStrategy,
            DeepCheck);

        internal CheckState<T> WithNextGenParameters(GenParameters genParameters) => new CheckState<T>(
            Property,
            RequestedIterations,
            ShrinkLimit,
            CompletedIterations,
            Discards,
            Shrinks,
            CounterexampleStateHistory,
            genParameters,
            ResizeStrategy,
            DeepCheck);
    }
}
