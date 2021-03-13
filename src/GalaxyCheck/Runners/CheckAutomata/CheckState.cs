using System.Collections.Immutable;
using System.Linq;
using static GalaxyCheck.CheckExtensions;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public record CheckState<T>(
        Property<T> Property,
        int RequestedIterations,
        int CompletedIterations,
        int Discards,
        int Shrinks,
        ImmutableList<CounterexampleState<T>> CounterexampleStateHistory,
        GenParameters NextParameters,
        ResizeStrategy<T> ResizeStrategy)
    {
        internal static CheckState<T> Create(
            Property<T> property,
            int requestedIterations,
            GenParameters initialParameters,
            ResizeStrategy<T> resizeStrategy) =>
                new CheckState<T>(
                    property,
                    requestedIterations,
                    0,
                    0,
                    0,
                    ImmutableList.Create<CounterexampleState<T>>(),
                    initialParameters,
                    resizeStrategy);

        public CounterexampleState<T>? Counterexample => CounterexampleStateHistory
            .OrderBy(c => c.ExampleSpace.Current.Distance)
            .ThenByDescending(c => c.RepeatParameters.Size.Value) // Bigger sizes are more likely to normalize
            .FirstOrDefault();

        internal CheckState<T> IncrementCompletedIterations() => new CheckState<T>(
            Property,
            RequestedIterations,
            CompletedIterations + 1,
            Discards,
            Shrinks,
            CounterexampleStateHistory,
            NextParameters,
            ResizeStrategy);

        internal CheckState<T> IncrementDiscards() => new CheckState<T>(
            Property,
            RequestedIterations,
            CompletedIterations,
            Discards + 1,
            Shrinks,
            CounterexampleStateHistory,
            NextParameters,
            ResizeStrategy);

        internal CheckState<T> IncrementShrinks() => new CheckState<T>(
            Property,
            RequestedIterations,
            CompletedIterations,
            Discards,
            Shrinks + 1,
            CounterexampleStateHistory,
            NextParameters,
            ResizeStrategy);

        internal CheckState<T> AddCounterexample(CounterexampleState<T> counterexampleState) => new CheckState<T>(
            Property,
            RequestedIterations,
            CompletedIterations,
            Discards,
            Shrinks,
            Enumerable.Concat(new[] { counterexampleState }, CounterexampleStateHistory).ToImmutableList(),
            NextParameters,
            ResizeStrategy);

        internal CheckState<T> WithNextGenParameters(GenParameters genParameters) => new CheckState<T>(
            Property,
            RequestedIterations,
            CompletedIterations,
            Discards,
            Shrinks,
            CounterexampleStateHistory,
            genParameters,
            ResizeStrategy);
    }
}
