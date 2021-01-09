using GalaxyCheck.Abstractions;
using GalaxyCheck.Sizing;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
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
        public ISize Size { get; init; }

        /// <summary>
        /// The initial RNG that seeded this check.
        /// </summary>
        public IRng InitialRng { get; init; }

        /// <summary>
        /// The initial RNG that seeded this check.
        /// </summary>
        public ISize InitialSize { get; init; }

        public IRng NextRng { get; init; }

        public ISize NextSize { get; init; }

        public int RandomnessConsumption => NextRng.Order - InitialRng.Order;

        internal CheckResult(
            CheckResultState<T> state,
            int iterations,
            int discards,
            IRng rng,
            ISize size,
            IRng initialRng,
            ISize initialSize,
            IRng nextRng,
            ISize nextSize)
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

        public sealed record Falsified<T>(T Value, decimal Distance, IEnumerable<int> Path) : CheckResultState<T>();

        public sealed record Error<T>() : CheckResultState<T>();
    }

    public static class PropertyCheckExtensions
    {
        public static CheckResult<T> Check<T>(this IProperty<T> property, RunConfig? config = null)
        {
            config ??= new RunConfig();
            var rng = config.Rng;
            var iterations = config.Iterations ?? 100;
            var size = config.Size ?? Size.MinValue;

            var currentResult = new CheckResult<T>(
                new CheckResultState.Unfalsified<T>(), 0, 0, rng, size, rng, size, rng, size);

            while (currentResult.Iterations < iterations)
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

        private static IEnumerable<GenIteration<PropertyIteration<T>>> CheckOnce<T>(IProperty<T> property, IRng rng, ISize size)
        {
            foreach (var iteration in property.Advanced.Run(rng, size))
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
            IEnumerable<GenIteration<PropertyIteration<T>>> iterations)
        {
            return iterations.Aggregate(
                previousResult,
                (result, iteration) => iteration.Match(
                    onInstance: instance => MergeInstanceIntoCheckResult(result, instance),
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
}
