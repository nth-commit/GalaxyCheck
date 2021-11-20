using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.ExampleSpaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal record CheckStateContext<T>
    {
        public IGen<Test<T>> Property { get; private init; }

        public int RequestedIterations { get; private init; }

        public int ShrinkLimit { get; private init; }

        public int CompletedIterations { get; private init; }

        public int Discards { get; private init; }

        public int Shrinks { get; private init; }

        public ImmutableList<CounterexampleContext<T>> CounterexampleContextHistory { get; private init; }

        public GenParameters NextParameters { get; private init; }

        public bool DeepCheck { get; private init; }

        private CheckStateContext(
            IGen<Test<T>> property,
            int requestedIterations,
            int shrinkLimit,
            int completedIterations,
            int discards,
            int shrinks,
            ImmutableList<CounterexampleContext<T>> counterexampleContextHistory,
            GenParameters nextParameters,
            bool deepCheck)
        {
            Property = property;
            RequestedIterations = requestedIterations;
            ShrinkLimit = shrinkLimit;
            CompletedIterations = completedIterations;
            Discards = discards;
            Shrinks = shrinks;
            CounterexampleContextHistory = counterexampleContextHistory;
            NextParameters = nextParameters;
            DeepCheck = deepCheck;
        }

        public CheckStateContext(
            IGen<Test<T>> property,
            int requestedIterations,
            int shrinkLimit,
            GenParameters initialParameters,
            bool deepCheck) : this(
                property,
                requestedIterations,
                shrinkLimit,
                0,
                0,
                0,
                ImmutableList.Create<CounterexampleContext<T>>(),
                initialParameters,
                deepCheck)
        {
        }

        public CounterexampleContext<T>? Counterexample => CounterexampleContextHistory
            .OrderBy(c => c.ExampleSpace.Current.Distance)
            .ThenByDescending(c => c.ReplayParameters.Size.Value) // Bigger sizes are more likely to normalize
            .FirstOrDefault();

        public CheckStateContext<T> IncrementCompletedIterations() => this with
        {
            CompletedIterations = CompletedIterations + 1
        };

        public CheckStateContext<T> IncrementDiscards() => this with
        {
            Discards = Discards + 1
        };

        public CheckStateContext<T> IncrementShrinks() => this with
        {
            Shrinks = Shrinks + 1
        };

        public CheckStateContext<T> AddCounterexample(CounterexampleContext<T> counterexampleContext) => this with
        {
            CounterexampleContextHistory = Enumerable
                .Concat(new[] { counterexampleContext }, CounterexampleContextHistory)
                .ToImmutableList()
        };

        public CheckStateContext<T> WithNextGenParameters(GenParameters genParameters) => this with
        {
            NextParameters = genParameters
        };
    }

    internal record ResizeStrategyInformation<T>(
        CheckStateContext<T> CheckStateContext,
        CounterexampleContext<T>? CounterexampleContext,
        IGenInstance<Test<T>> Iteration);

    internal delegate Size ResizeStrategy<T>(ResizeStrategyInformation<T> info);

    internal record CounterexampleContext<T>(
        IExampleSpace<Test<T>> TestExampleSpace,
        IEnumerable<Lazy<IExampleSpace<object>?>> ExampleSpaceHistory,
        GenParameters ReplayParameters,
        IEnumerable<int> ReplayPath,
        Exception? Exception)
    {
        public IExampleSpace<T> ExampleSpace => TestExampleSpace.Map(ex => ex.Input);

        public T Value => ExampleSpace.Current.Value;

        public object?[] PresentationalValue
        {
            get
            {
                var test = TestExampleSpace.Current.Value;
                var arity = test.PresentedInput?.Length;
                return arity > 0 ? test.PresentedInput! : PresentationInferrer.InferValue(ExampleSpaceHistory);
            }
        }
    }
}
