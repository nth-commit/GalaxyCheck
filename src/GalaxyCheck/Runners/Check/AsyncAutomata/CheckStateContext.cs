using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.ExampleSpaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Runners.Check.AsyncAutomata
{
    internal record CheckStateContext<T>
    {
        public IGen<AsyncTest<T>> Property { get; private init; }

        public int RequestedIterations { get; private init; }

        public int ShrinkLimit { get; private init; }

        public int CompletedIterationsUntilCounterexample { get; private init; }

        public int CompletedIterationsAfterCounterexample { get; private init; }

        public int CompletedIterations => CompletedIterationsUntilCounterexample + CompletedIterationsAfterCounterexample;

        public int Discards { get; private init; }

        public int ConsecutiveDiscards { get; private init; } = 0;

        public int ConsecutiveLateDiscards { get; private init; } = 0;

        public int Shrinks { get; private init; }

        public ImmutableList<CounterexampleContext<T>> CounterexampleContextHistory { get; private init; }

        public GenParameters NextParameters { get; private init; }

        public bool DeepCheck { get; private init; }

        private CheckStateContext(
            IGen<AsyncTest<T>> property,
            int requestedIterations,
            int shrinkLimit,
            int completedIterationsUntilCounterexample,
            int completedIterationsAfterCounterexample,
            int discards,
            int shrinks,
            ImmutableList<CounterexampleContext<T>> counterexampleContextHistory,
            GenParameters nextParameters,
            bool deepCheck)
        {
            Property = property;
            RequestedIterations = requestedIterations;
            ShrinkLimit = shrinkLimit;
            CompletedIterationsUntilCounterexample = completedIterationsUntilCounterexample;
            CompletedIterationsAfterCounterexample = completedIterationsAfterCounterexample;
            Discards = discards;
            Shrinks = shrinks;
            CounterexampleContextHistory = counterexampleContextHistory;
            NextParameters = nextParameters;
            DeepCheck = deepCheck;
        }

        public CheckStateContext(
            IGen<AsyncTest<T>> property,
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

        public CheckStateContext<T> IncrementCompletedIterations() => (this with
        {
            CompletedIterationsUntilCounterexample = Counterexample == null
                ? CompletedIterationsUntilCounterexample + 1
                : CompletedIterationsUntilCounterexample,
            CompletedIterationsAfterCounterexample = Counterexample == null
                ? CompletedIterationsAfterCounterexample
                : CompletedIterationsAfterCounterexample + 1,
        }).ResetConsecutiveDiscards();

        public CheckStateContext<T> IncrementDiscards(bool wasLateDiscard) => this with
        {
            Discards = Discards + 1,
            ConsecutiveDiscards = ConsecutiveDiscards + 1,
            ConsecutiveLateDiscards = wasLateDiscard ? ConsecutiveLateDiscards + 1 : 0
        };

        public CheckStateContext<T> IncrementShrinks() => this with
        {
            Shrinks = Shrinks + 1
        };

        public CheckStateContext<T> ResetConsecutiveDiscards() => this with
        {
            ConsecutiveDiscards = 0,
            ConsecutiveLateDiscards = 0
        };

        public CheckStateContext<T> AddCounterexample(CounterexampleContext<T> counterexampleContext) => this with
        {
            CounterexampleContextHistory = (new[] { counterexampleContext }).Concat(CounterexampleContextHistory)
                .ToImmutableList()
        };

        public CheckStateContext<T> WithNextGenParameters(GenParameters genParameters) => this with
        {
            NextParameters = genParameters
        };
    }

    internal record CounterexampleContext<T>(
       IExampleSpace<AsyncTest<T>> TestExampleSpace,
       IEnumerable<Lazy<IExampleSpace<object>?>> ExampleSpaceHistory,
       GenParameters ReplayParameters,
       IEnumerable<int> ReplayPath,
       Exception? Exception)
    {
        public IExampleSpace<T> ExampleSpace => TestExampleSpace.Map(ex => ex.Input);

        public T Value => ExampleSpace.Current.Value;

        public IReadOnlyList<object?> PresentationalValue
        {
            get
            {
                var test = TestExampleSpace.Current.Value;
                var arity = test.PresentedInput?.Count;
                return arity > 0 ? test.PresentedInput! : PresentationInferrer.InferValue(ExampleSpaceHistory);
            }
        }
    }
}
