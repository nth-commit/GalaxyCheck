using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Random;
using GalaxyCheck.Internal.Sizing;
using GalaxyCheck.Internal.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck
{
    public record CheckResult<T>
    {
        public int Iterations { get; init; }

        public int Discards { get; init; }

        public int IterationsAfterFalsified { get; init; }

        public int DiscardsAfterFalsified { get; init; }

        public bool Falsified { get; init; }

        public CheckCounterexample? Counterexample { get; init; }

        public ImmutableList<CheckIteration> Checks { get; init; }

        public IRng InitialRng { get; init; }

        public Size InitialSize { get; init; }

        public IRng NextRng { get; init; }

        public Size NextSize { get; init; }

        public int RandomnessConsumption => NextRng.Order - InitialRng.Order;

        public CheckResult(
            int iterations,
            int discards,
            int iterationsAfterFalsified,
            int discardsAfterFalsified,
            bool falsified,
            CheckCounterexample? counterexample,
            ImmutableList<CheckIteration> checks,
            IRng initialRng,
            Size initialSize,
            IRng nextRng,
            Size nextSize)
        {
            Iterations = iterations;
            Discards = discards;
            IterationsAfterFalsified = iterationsAfterFalsified;
            DiscardsAfterFalsified = discardsAfterFalsified;
            Falsified = falsified;
            Counterexample = counterexample;
            Checks = checks;
            InitialRng = initialRng;
            InitialSize = initialSize;
            NextRng = nextRng;
            NextSize = nextSize;
        }

        public record CheckIteration(
            T Value,
            decimal Distance,
            Exception? Exception,
            IRng InitialRng,
            Size InitialSize,
            IRng NextRng,
            Size NextSize,
            ImmutableList<int> Path,
            bool IsCounterexample,
            bool IsShrink,
            bool IsDiscard);

        public record CheckCounterexample(
            T Value,
            decimal Distance,
            Exception? Exception,
            IRng Rng,
            Size Size,
            ImmutableList<int> Path);
    }

    public static class CheckExtensions
    {
        public static CheckResult<T> Check<T>(
            this IProperty<T> property,
            int? iterations = null,
            int? seed = null,
            int? size = null)
        {
            var initialRng = seed == null ? Rng.Spawn() : Rng.Create(seed.Value);
            var initialSize = size == null ? Size.MinValue : new Size(size.Value);

            var checks = CheckOnce(property, iterations ?? 100, initialRng, initialSize).ToImmutableList();
            var checkAggregation = AggregateCheckIterations(checks.ToImmutableList());
            var nextRng = checks.LastOrDefault()?.NextRng ?? initialRng;
            var nextSize = checks.LastOrDefault()?.NextSize ?? initialSize;

            return new CheckResult<T>(
                checkAggregation.IterationCount,
                checkAggregation.DiscardCount,
                0,
                0,
                checkAggregation.Counterexample != null,
                checkAggregation.Counterexample,
                checkAggregation.Checks,
                initialRng,
                initialSize,
                nextRng,
                nextSize);
        }

        private static IEnumerable<CheckResult<T>.CheckIteration> CheckOnce<T>(
            IProperty<T> property,
            int iterations,
            IRng initialRng,
            Size initialSize)
        {
            foreach (var instance in Run(property, initialRng, initialSize).Take(iterations))
            {
                var isFalsified = false;

                foreach (var exploration in instance.ExampleSpace.Explore(x => x.Func(x.Input)))
                {
                    isFalsified = isFalsified || exploration.IsCounterexample;

                    yield return new CheckResult<T>.CheckIteration(
                        exploration.Example.Value.Input,
                        exploration.Example.Distance,
                        exploration.CounterexampleDetails?.Exception,
                        instance.InitialRng,
                        instance.InitialSize,
                        instance.NextRng,
                        instance.NextSize,
                        exploration.Path.ToImmutableList(),
                        IsCounterexample: exploration.IsCounterexample,
                        IsShrink: exploration.IsShrink,
                        IsDiscard: false);
                }

                if (isFalsified)
                {
                    break;
                }
            }
        }

        private static IEnumerable<GenInstance<PropertyIteration<T>>> Run<T>(IProperty<T> property, IRng initialRng, Size initialSize)
        {
            IEnumerable<GenIteration<PropertyIteration<T>>> RunProperty(IRng rng, Size size) =>
                property.Advanced
                    .Run(rng, size)
                    .TakeWhileInclusive(iteration => iteration is not GenInstance<PropertyIteration<T>>);

            return EnumerableExtensions
                .UnfoldManyInfinite(
                    RunProperty(initialRng, initialSize),
                    (lastIteration) => RunProperty(lastIteration.NextRng, lastIteration.NextSize.Increment()))
                .ScanInParallel(
                    new PropertyIterationCounts(0, 0),
                    (lastState, iteration) => iteration.Match(
                        onInstance: _ => lastState.IncrementInstances(),
                        onDiscard: _ => lastState.IncrementDiscards(),
                        onError: error => throw new Exceptions.GenErrorException(error.GenName, error.Message)
                    ))
                .Tap(x =>
                {
                    if (x.state.Discards >= 1000)
                    {
                        throw new Exceptions.GenExhaustionException();
                    }
                })
                .Select(x => x.element)
                .OfType<GenInstance<PropertyIteration<T>>>();
        }

        private static CheckIterationAggregation<T> AggregateCheckIterations<T>(
            ImmutableList<CheckResult<T>.CheckIteration> checks)
        {
            var iterationCount = 0;
            var discardCount = 0;
            CheckResult<T>.CheckCounterexample? counterexample = null;

            foreach (var check in checks)
            {
                if (check.IsDiscard)
                {
                    discardCount++;
                }
                else
                {
                    iterationCount++;
                }

                if (check.IsCounterexample)
                {
                    counterexample = new CheckResult<T>.CheckCounterexample(
                        check.Value,
                        check.Distance,
                        check.Exception,
                        check.InitialRng,
                        check.InitialSize,
                        check.Path);
                }
            }

            return new CheckIterationAggregation<T>(
                iterationCount,
                discardCount,
                counterexample,
                checks);
        }

        private record PropertyIterationCounts(int Instances, int Discards)
        {
            public PropertyIterationCounts IncrementInstances() => new PropertyIterationCounts(Instances + 1, Discards);

            public PropertyIterationCounts IncrementDiscards() => new PropertyIterationCounts(Instances, Discards + 1);
        }

        private record CheckIterationAggregation<T>(
            int IterationCount,
            int DiscardCount,
            CheckResult<T>.CheckCounterexample? Counterexample,
            ImmutableList<CheckResult<T>.CheckIteration> Checks);
    }
}
