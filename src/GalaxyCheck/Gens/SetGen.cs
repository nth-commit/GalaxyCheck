namespace GalaxyCheck
{
    using GalaxyCheck.Gens;
    using GalaxyCheck.Gens.Internal;
    using System.Collections.Generic;

    public static partial class Gen
    {
        public static ISetGen<T> Set<T>(IGen<T> elementGen) => new SetGen<T>(RangeIntention.Unspecified());
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Creates a generator that produces sets, the elements of which are produced by the given generator. By
        /// default, the generator produces sets ranging from count 0 to 20 - but this can be configured using the
        /// builder methods on <see cref="ISetGen{T}"/>.
        /// </summary>
        /// <param name="elementGen">The generator used to produce the elements of the set.</param>
        /// <returns>The new generator.</returns>
        public static ISetGen<T> SetOf<T>(this IGen<T> gen) => Gen.Set(gen);

        /// <summary>
        /// Constrains the generator so that it only produces sets with counts within the supplied range (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static ISetGen<T> WithCountBetween<T>(this ISetGen<T> gen, int x, int y)
        {
            var minCount = x > y ? y : x;
            var maxCount = x > y ? x : y;
            return gen.WithCountGreaterThanEqual(minCount).WithCountLessThanEqual(maxCount);
        }

        /// <summary>
        /// Constrains the generator so that it only produces sets with greater than the given count.
        /// </summary>
        /// <param name="exclusiveMinCount">The minimum count that generated sets should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static ISetGen<T> WithCountGreaterThan<T>(this ISetGen<T> gen, int exclusiveMinCount) =>
            gen.WithCountGreaterThanEqual(exclusiveMinCount + 1);

        /// <summary>
        /// Constrains the generator so that it only produces sets less than the given count.
        /// </summary>
        /// <param name="exclusiveMaxCount">The maximum count that generated sets should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static ISetGen<T> WithCountLessThan<T>(this ISetGen<T> gen, int exclusiveMaxCount) =>
            gen.WithCountLessThanEqual(exclusiveMaxCount - 1);
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using System.Collections.Generic;

    public interface ISetGen<T> : IGen<IReadOnlyCollection<T>>
    {
        /// <summary>
        /// Constrains the generator so that it only produces sets with the given count.
        /// </summary>
        /// <param name="count">The count to constrain generated sets to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        ISetGen<T> WithCount(int count);

        /// <summary>
        /// Constrains the generator so that it only produces sets with at least the given count.
        /// </summary>
        /// <param name="minCount">The minimum count that generated sets should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        ISetGen<T> WithCountGreaterThanEqual(int minCount);

        /// <summary>
        /// Constrains the generator so that it only produces sets with at most the given count.
        /// </summary>
        /// <param name="maxCount">The maximum count that generated sets should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        ISetGen<T> WithCountLessThanEqual(int maxCount);

        /// <summary>
        /// Modifies how the generator biases counts of produced sets with respect to the size parameter.
        /// </summary>
        /// <returns>A new generator with the biasing effect applied.</returns>
        ISetGen<T> WithCountBias(Gen.Bias bias);
    }

    internal record SetGen<T>(
        RangeIntention Count) : GenProvider<IReadOnlyCollection<T>>, ISetGen<T>
    {
        public ISetGen<T> WithCount(int count) => this with { Count = RangeIntention.Exact(count) };

        public ISetGen<T> WithCountGreaterThanEqual(int minCount) => this with { Count = Count.WithMinimum(minCount) };

        public ISetGen<T> WithCountLessThanEqual(int maxCount) => this with { Count = Count.WithMaximum(maxCount) };

        public ISetGen<T> WithCountBias(Gen.Bias bias)
        {
            throw new System.NotImplementedException();
        }

        protected override IGen<IReadOnlyCollection<T>> Get
        {
            get
            {
                var countErrorGen = ValidateCount(Count);

                if (countErrorGen != null)
                {
                    return countErrorGen;
                }

                return Gen.Constant(new List<T>());
            }
        }

        private static IGen<IReadOnlyCollection<T>>? ValidateCount(RangeIntention count)
        {
            return count.Match(
                onExact: count => ValidateCountNotNegative("count", count),
                onUnspecified: () => null,
                onBounded: (minCount, maxCount) =>
                    (minCount == null ? null : ValidateCountNotNegative("minCount", minCount.Value)) ??
                    (maxCount == null ? null : ValidateCountNotNegative("maxCount", maxCount.Value)) ??
                    (minCount != null && maxCount != null && minCount.Value > maxCount.Value
                        ? Error($"'minCount' cannot be greater than 'maxCount'")
                        : null));
        }

        private static IGen<IReadOnlyCollection<T>>? ValidateCountNotNegative(string paramName, int count) =>
            count >= 0 ? null : Error($"'{paramName}' cannot be negative");

        private static IGen<IReadOnlyCollection<T>> Error(string message) =>
            Gen.Advanced.Error<IReadOnlyCollection<T>>(nameof(SetGen<T>), message);
    }
}