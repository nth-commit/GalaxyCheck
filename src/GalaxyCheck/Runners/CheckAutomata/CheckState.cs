using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.ExampleSpaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal record CheckState<T>(
        IGen<Test<T>> Property,
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
        public static CheckState<T> Create(
            IGen<Test<T>> property,
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

        public CheckState<T> IncrementCompletedIterations() => new CheckState<T>(
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

        public CheckState<T> IncrementDiscards() => new CheckState<T>(
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

        public CheckState<T> IncrementShrinks() => new CheckState<T>(
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

        public CheckState<T> AddCounterexample(CounterexampleState<T> counterexampleState) => new CheckState<T>(
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

        public CheckState<T> WithNextGenParameters(GenParameters genParameters) => new CheckState<T>(
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

    internal record ResizeStrategyInformation<T>(
        CheckState<T> CheckState,
        CounterexampleState<T>? CounterexampleState,
        IGenIteration<Test<T>> Iteration);

    internal delegate Size ResizeStrategy<T>(ResizeStrategyInformation<T> info);

    internal record CounterexampleState<T>(
        IExampleSpace<T> ExampleSpace,
        IEnumerable<Lazy<IExampleSpace<object>?>> ExampleSpaceHistory,
        GenParameters ReplayParameters,
        IEnumerable<int> ReplayPath,
        Exception? Exception)
    {
        public T Value => ExampleSpace.Current.Value;

        public object?[] PresentationalValue => PresentationInferrer.InferValue(ExampleSpaceHistory);
    }
}
