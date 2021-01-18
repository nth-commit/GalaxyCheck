using GalaxyCheck.Internal.Utility;
using System.Collections.Generic;
using System.Linq;
using GalaxyCheck.Internal.Sizing;
using GalaxyCheck.Internal.Random;
using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using System;

namespace GalaxyCheck
{
    public static class CheckExtensions
    {
        public static CheckResult<T> Check<T>(
            this IProperty<T> property,
            int iterations = 100,
            int? seed = null,
            int? size = null)
        {
            var firstCheckOnce = CheckOnce(
                property,
                iterations: iterations,
                rng: seed == null ? Rng.Spawn() : Rng.Create(seed.Value),
                size: size == null ? Size.MinValue : new Size(size.Value));

            var firstCheck = new CheckResult<T>(
                firstCheckOnce.State,
                firstCheckOnce.Iterations,
                firstCheckOnce.Discards,
                firstCheckOnce.Rng,
                firstCheckOnce.Size,
                firstCheckOnce.InitialRng,
                firstCheckOnce.InitialSize,
                firstCheckOnce.NextRng,
                firstCheckOnce.NextSize,
                0,
                0);

            if (firstCheckOnce.State is not CheckResultState.Falsified<T> falsified ||
                falsified.Distance == 0)
            {
                return firstCheck;
            }

            return EnumerableExtensions
                .UnfoldInfinite(firstCheck, check =>
                {
                    var nextCheckOnce = CheckAgain(property, iterations, check);
                    return MergeCheckOnceIntoCheck(check, nextCheckOnce);
                })
                .Take(5)
                .Last();
        }

        private static CheckResult<T> MergeCheckOnceIntoCheck<T>(
            CheckResult<T> check,
            CheckOnceResult<T> checkOnce)
        {
            if (checkOnce.State is CheckResultState.Falsified<T> falsified &&
                (check.State is not CheckResultState.Falsified<T> originalFalsified ||
                 falsified.Distance < originalFalsified.Distance))
            {
                return new CheckResult<T>(
                    checkOnce.State,
                    check.Iterations,
                    check.Discards,
                    checkOnce.Rng,
                    checkOnce.Size,
                    check.InitialRng,
                    check.InitialSize,
                    checkOnce.NextRng,
                    checkOnce.NextSize,
                    check.IterationsAfterFalsified + checkOnce.Iterations,
                    check.DiscardsAfterFalsified + checkOnce.Discards);
            }

            return new CheckResult<T>(
                check.State,
                check.Iterations,
                check.Discards,
                check.Rng,
                check.Size,
                check.InitialRng,
                check.InitialSize,
                checkOnce.NextRng,
                checkOnce.NextSize,
                check.IterationsAfterFalsified + checkOnce.Iterations,
                check.DiscardsAfterFalsified + checkOnce.Discards);
        }

        private static CheckOnceResult<T> CheckAgain<T>(
            IProperty<T> property,
            int iterations,
            CheckResult<T> lastCheckResult) =>
                CheckOnce(property, iterations, lastCheckResult.NextRng, lastCheckResult.NextSize.BigIncrement());

        private static CheckOnceResult<T> CheckOnce<T>(
            IProperty<T> property,
            int iterations,
            IRng rng,
            Size size)
        {
            var currentResult = new CheckOnceResult<T>(
                new CheckResultState.Unfalsified<T>(), 0, 0, rng, size, rng, size, rng, size);

            while (currentResult.Iterations < iterations)
            {
                var nextIterationResults = RunUntilInstanceOrError(
                    property,
                    currentResult.NextRng,
                    currentResult.NextSize.Increment());
                var nextResult = AggregateIterationsIntoResult(currentResult, nextIterationResults);

                if (nextResult.State is not CheckResultState.Unfalsified<T>)
                {
                    return nextResult;
                }

                currentResult = nextResult;
            }

            return currentResult;
        }

        private static IEnumerable<GenIteration<PropertyIteration<T>>> RunUntilInstanceOrError<T>(
            IProperty<T> property,
            IRng rng,
            Size size)
        {
            var stream = property.Advanced
                .Run(rng, size)
                .WithConsecutiveDiscardCount()
                .Select(x =>
                {
                    if (x.consecutiveDiscards > 1000)
                    {
                        throw new Exceptions.GenExhaustionException();
                    }

                    return x.iteration;
                });

            foreach (var iteration in stream)
            {
                yield return iteration;

                // TODO: Don't break on a discard
                var shouldBreak = iteration.Match(
                    onInstance: _ => true,
                    onDiscard: _ => false,
                    onError: _ => true);

                if (shouldBreak)
                {
                    break;
                }
            }
        }

        private static CheckOnceResult<T> AggregateIterationsIntoResult<T>(
            CheckOnceResult<T> previousResult,
            IEnumerable<GenIteration<PropertyIteration<T>>> iterations)
        {
            return iterations.Aggregate(
                previousResult,
                (result, iteration) => iteration.Match(
                    onInstance: instance => MergeInstanceIntoCheckResult(result, instance),
                    onDiscard: _ => result,
                    onError: error => new CheckOnceResult<T>(
                        new CheckResultState.Error<T>(),
                        result.Iterations,
                        result.Discards,
                        error.InitialRng,
                        result.InitialSize,
                        result.InitialRng,
                        result.InitialSize,
                        error.NextRng,
                        result.InitialSize)));
        }

        private static CheckOnceResult<T> MergeInstanceIntoCheckResult<T>(
            CheckOnceResult<T> checkResult, GenInstance<PropertyIteration<T>> instance)
        {
            var smallestCounterexample = instance.ExampleSpace
                .Counterexamples(x => x.Func(x.Input))
                .LastOrDefault();

            CheckResultState<T> state = smallestCounterexample == null
                ? new CheckResultState.Unfalsified<T>()
                : new CheckResultState.Falsified<T>(
                    smallestCounterexample.Value.Input,
                    smallestCounterexample.Distance,
                    smallestCounterexample.Exception,
                    smallestCounterexample.Path);

            return new CheckOnceResult<T>(
                state,
                checkResult.Iterations + 1,
                checkResult.Discards,
                instance.InitialRng,
                instance.InitialSize,
                checkResult.InitialRng,
                checkResult.InitialSize,
                instance.NextRng,
                instance.NextSize);
        }
    }

    public record CheckResult<T>
    {
        public CheckResultState<T> State { get; init; }

        public int Iterations { get; init; }

        public int Discards { get; init; }

        public int IterationsAfterFalsified { get; init; }

        public int DiscardsAfterFalsified { get; init; }

        /// <summary>
        /// The RNG required to repeat the final iteration of the property.
        /// </summary>
        public IRng Rng { get; init; }

        /// <summary>
        /// The size required to repeat the final iteration of the property.
        /// </summary>
        public Size Size { get; init; }

        /// <summary>
        /// The initial RNG that seeded this check.
        /// </summary>
        public IRng InitialRng { get; init; }

        /// <summary>
        /// The initial RNG that seeded this check.
        /// </summary>
        public Size InitialSize { get; init; }

        public IRng NextRng { get; init; }

        public Size NextSize { get; init; }

        public int RandomnessConsumption => NextRng.Order - InitialRng.Order;

        internal CheckResult(
            CheckResultState<T> state,
            int iterations,
            int discards,
            IRng rng,
            Size size,
            IRng initialRng,
            Size initialSize,
            IRng nextRng,
            Size nextSize,
            int iterationsAfterFalsified,
            int discardsAfterFalsified)
        {
            State = state;
            Iterations = iterations;
            Discards = discards;
            Rng = rng;
            Size = size;
            InitialRng = initialRng;
            InitialSize = initialSize;
            NextRng = nextRng;
            NextSize = nextSize;
            IterationsAfterFalsified = iterationsAfterFalsified;
            DiscardsAfterFalsified = discardsAfterFalsified;
        }
    }

    public record CheckOnceResult<T>
    {
        public CheckResultState<T> State { get; init; }

        public int Iterations { get; init; }

        public int Discards { get; init; }

        public int IterationsAfterFalsified { get; init; }

        public int DiscardsAfterFalsified { get; init; }

        /// <summary>
        /// The RNG required to repeat the final iteration of the property.
        /// </summary>
        public IRng Rng { get; init; }

        /// <summary>
        /// The size required to repeat the final iteration of the property.
        /// </summary>
        public Size Size { get; init; }

        /// <summary>
        /// The initial RNG that seeded this check.
        /// </summary>
        public IRng InitialRng { get; init; }

        /// <summary>
        /// The initial RNG that seeded this check.
        /// </summary>
        public Size InitialSize { get; init; }

        public IRng NextRng { get; init; }

        public Size NextSize { get; init; }

        public int RandomnessConsumption => NextRng.Order - InitialRng.Order;

        internal CheckOnceResult(
            CheckResultState<T> state,
            int iterations,
            int discards,
            IRng rng,
            Size size,
            IRng initialRng,
            Size initialSize,
            IRng nextRng,
            Size nextSize)
        {
            State = state;
            Iterations = iterations;
            Discards = discards;
            Rng = rng;
            Size = size;
            InitialRng = initialRng;
            InitialSize = initialSize;
            NextRng = nextRng;
            NextSize = nextSize;
        }
    }

    public abstract record CheckResultState<T>
    {
    }

    public static class CheckResultState
    {
        public sealed record Unfalsified<T>() : CheckResultState<T>();

        public sealed record Falsified<T>(T Value, decimal Distance, Exception? Exception, IEnumerable<int> Path) : CheckResultState<T>();

        public sealed record Error<T>() : CheckResultState<T>();
    }
}
