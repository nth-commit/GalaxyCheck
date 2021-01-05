using GalaxyCheck.Abstractions;
using GalaxyCheck.Random;
using GalaxyCheck.Sizing;
using GalaxyCheck.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    public abstract record CheckResult<T>
    {
        public int Runs { get; init; }

        public int Discards { get; init; }

        public IRng Rng { get; init; }

        public ISize Size { get; init; }

        public IRng InitialRng { get; init; }

        public ISize InitialSize { get; init; }

        public IRng NextRng { get; init; }

        public ISize NextSize { get; init; }

        public int RandomnessConsumption => NextRng.Order - InitialRng.Order;

        internal CheckResult(int runs, int discards, IRng rng, ISize size, IRng initialRng, ISize initialSize, IRng nextRng, ISize nextSize)
        {
            Runs = runs;
            Discards = discards;
            Rng = rng;
            Size = size;
            InitialRng = initialRng;
            InitialSize = initialSize;
            NextRng = nextRng;
            NextSize = nextSize;
        }
    }

    public static class CheckResult
    {
        public sealed record Unfalsified<T>(int Runs, int Discards, IRng Rng, ISize Size, IRng InitialRng, ISize InitialSize, IRng NextRng, ISize NextSize)
            : CheckResult<T>(Runs, Discards, Rng, Size, InitialRng, InitialSize, NextRng, NextSize);

        public sealed record Falsified<T>(int Runs, int Discards, IRng Rng, ISize Size, IRng InitialRng, ISize InitialSize, IRng NextRng, ISize NextSize) :
            CheckResult<T>(Runs, Discards, Rng, Size, InitialRng, InitialSize, NextRng, NextSize);

        public sealed record Error<T>(int Runs, int Discards, IRng Rng, ISize Size, IRng InitialRng, ISize InitialSize, IRng NextRng, ISize NextSize) :
            CheckResult<T>(Runs, Discards, Rng, Size, InitialRng, InitialSize, NextRng, NextSize);
    }

    public static class PropertyCheckExtensions
    {
        public static CheckResult<T> Check<T>(this IProperty<T> property, RunConfig? config = null)
        {
            config = config ?? new RunConfig();
            var rng = config.Rng;
            var iterations = config.Iterations ?? 100;
            var size = config.Size ?? new Size(100);

            var initialResult = new CheckResult.Unfalsified<T>(0, 0, rng, size, rng, size, rng, size);

            return EnumerableExtensions
                .Unfold<CheckResult<T>>(
                    initialResult,
                    previousResult =>
                    {
                        if (previousResult is not CheckResult.Unfalsified<T> unfalsifiedResult)
                        {
                            return new Option.None<CheckResult<T>>();
                        }

                        var iterations = CheckOnce(property, unfalsifiedResult.NextRng, unfalsifiedResult.NextSize);
                        var nextResult = AggregateIterationsIntoResult(previousResult, iterations);

                        return new Option.Some<CheckResult<T>>(nextResult);
                    })
                .TakeWhile(r => r.Discards <= 1000 && r.Runs <= iterations)
                .Last();
        }

        private static IEnumerable<GenIteration<PropertyResult<T>>> CheckOnce<T>(IProperty<T> property, IRng rng, ISize size)
        {
            foreach (var iteration in property.Run(rng, size))
            {
                yield return iteration;

                // TODO: Don't break on a discard
                var shouldBreak = iteration.Match(
                    onInstance: _ => true,
                    onError: _ => true);

                if (shouldBreak)
                {
                    break;
                }
            }
        }

        private static CheckResult<T> AggregateIterationsIntoResult<T>(
            CheckResult<T> previousResult,
            IEnumerable<GenIteration<PropertyResult<T>>> iterations)
        {
            return iterations.Aggregate(
                previousResult,
                (result, iteration) => iteration.Match(
                    onInstance: instance => MergeInstanceIntoCheckResult(result, instance),
                    onError: error => new CheckResult.Error<T>(result.Runs, result.Discards, error.InitialRng, result.InitialSize, result.InitialRng, result.InitialSize, error.NextRng, result.InitialSize)));
        }

        private static CheckResult<T> MergeInstanceIntoCheckResult<T>(CheckResult<T> checkResult, GenInstance<PropertyResult<T>> instance)
        {
            var propertyResult = instance.ExampleSpace.Traverse().First().Value;
            return propertyResult is PropertyResult.Success<T>
                ? new CheckResult.Unfalsified<T>(checkResult.Runs + 1, checkResult.Discards, instance.InitialRng, checkResult.InitialSize, checkResult.InitialRng, checkResult.InitialSize, instance.NextRng, checkResult.InitialSize)
                : new CheckResult.Falsified<T>(checkResult.Runs + 1, checkResult.Discards, instance.InitialRng, checkResult.InitialSize, checkResult.InitialRng, checkResult.InitialSize, instance.NextRng, checkResult.InitialSize);
        }
    }
}
