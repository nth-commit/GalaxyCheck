﻿using GalaxyCheck.Internal.Utility;
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
            int? iterations = null,
            int? seed = null,
            int? size = null)
        {
            var resolvedIterations = iterations ?? 100;
            var resolvedRng = seed == null ? Rng.Spawn() : Rng.Create(seed.Value);
            var resolvedSize = size == null ? Size.MinValue : new Size(size.Value);

            var currentResult = new CheckResult<T>(
                new CheckResultState.Unfalsified<T>(), 0, 0, resolvedRng, resolvedSize, resolvedRng, resolvedSize, resolvedRng, resolvedSize);

            while (currentResult.Iterations < resolvedIterations)
            {
                var nextIterationResults = CheckOnce(
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

        private static IEnumerable<GenIteration<PropertyIteration<T>>> CheckOnce<T>(
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

        private static CheckResult<T> AggregateIterationsIntoResult<T>(
            CheckResult<T> previousResult,
            IEnumerable<GenIteration<PropertyIteration<T>>> iterations)
        {
            return iterations.Aggregate(
                previousResult,
                (result, iteration) => iteration.Match(
                    onInstance: instance => MergeInstanceIntoCheckResult(result, instance),
                    onDiscard: _ => result,
                    onError: error => new CheckResult<T>(
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

        private static CheckResult<T> MergeInstanceIntoCheckResult<T>(
            CheckResult<T> checkResult, GenInstance<PropertyIteration<T>> instance)
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

            return new CheckResult<T>(
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