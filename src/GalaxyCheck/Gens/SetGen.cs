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
        public static IGen<IReadOnlyCollection<T>> SetOf<T>(this IGen<T> elementGen) => Gen.Set(elementGen);
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