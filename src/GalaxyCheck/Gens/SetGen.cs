﻿namespace GalaxyCheck
{
    using GalaxyCheck.Gens;
    using GalaxyCheck.Gens.Internal;

    public static partial class Gen
    {
        public static ISetGen<T> Set<T>(IGen<T> elementGen) => new SetGen<T>(
            ElementGen: elementGen,
            Count: RangeIntention.Unspecified(),
            EnableCountLimit: null);
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
    using GalaxyCheck.Internal;
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

        /// <summary>
        /// <para>
        /// By default, trying to generate a set with a count greater than 1000 is disabled. This is a built-in
        /// safety mechanism to prevent confusing test failures or hangs. Huge sets can potentially blow out the
        /// performance footprint of a test to unattainable levels, because the generation performance for sets is
        /// O(n).
        /// </para>
        /// <para>
        /// It's quite easy to accidentally create a set generator for huge sets. For example, the following code
        /// might otherwise attempt to generate a list with count int.MaxValue:
        /// <code>
        /// Gen.Int32().GreaterThanEqual(0).SelectMany(count => Gen.Int32().SetOf().WithCount(count));
        /// </code>
        /// </para>
        /// </summary>
        /// <returns>A new generator with the limit disabled.</returns>
        ISetGen<T> DisableCountLimitUnsafe();
    }

    internal record SetGen<T>(
        IGen<T> ElementGen,
        RangeIntention Count,
        bool? EnableCountLimit) : GenProvider<IReadOnlyCollection<T>>, ISetGen<T>
    {
        public ISetGen<T> WithCount(int count) => this with { Count = RangeIntention.Exact(count) };

        public ISetGen<T> WithCountGreaterThanEqual(int minCount) => this with { Count = Count.WithMinimum(minCount) };

        public ISetGen<T> WithCountLessThanEqual(int maxCount) => this with { Count = Count.WithMaximum(maxCount) };

        public ISetGen<T> WithCountBias(Gen.Bias bias)
        {
            throw new System.NotImplementedException();
        }

        public ISetGen<T> DisableCountLimitUnsafe() => this with { EnableCountLimit = false };

        protected override IGen<IReadOnlyCollection<T>> Get => TryGetCountRange(Count).Match(
            fromLeft: range => range == null
                ? GenSetOfUnspecifiedCount(ElementGen, (0, 0), Gen.Bias.WithSize)
                : GenSetOfSpecifiedCount(ElementGen, range.Value, Gen.Bias.WithSize, EnableCountLimit ?? true),
            fromRight: gen => gen);

        private static IGen<IReadOnlyCollection<T>> GenSetOfSpecifiedCount(
            IGen<T> elementGen,
            (int minCount, int maxCount) range,
            Gen.Bias bias,
            bool enableCountLimit)
        {
            var (minCount, maxCount) = range;

            if (enableCountLimit && (minCount > 1000 || maxCount > 1000))
            {
                return CountLimitError();
            }

            return Gen.Constant(new List<T>());
        }

        private static IGen<IReadOnlyCollection<T>> GenSetOfUnspecifiedCount(
            IGen<T> elementGen,
            (int minCount, int maxCount) range,
            Gen.Bias bias)
        {
            return Gen.Constant(new List<T>());
        }

        private static Either<(int minCount, int maxCount)?, IGen<IReadOnlyCollection<T>>> TryGetCountRange(RangeIntention count)
        {
            Either<(int minCount, int maxCount)?, IGen<IReadOnlyCollection<T>>> CountError(string message) =>
                new Right<(int minCount, int maxCount)?, IGen<IReadOnlyCollection<T>>>(Error(message));

            Either<(int minCount, int maxCount)?, IGen<IReadOnlyCollection<T>>> FromUnspecified() =>
                new Left<(int minCount, int maxCount)?, IGen<IReadOnlyCollection<T>>>(null);

            Either<(int minCount, int maxCount)?, IGen<IReadOnlyCollection<T>>> FromExact(int count) =>
                count < 0 ? CountError("'count' cannot be negative") : (count, count);

            Either<(int minCount, int maxCount)?, IGen<IReadOnlyCollection<T>>> FromBounded(int? minCount, int? maxCount)
            {
                var resolvedMinCount = minCount ?? 0;
                var resolvedMaxCount = maxCount ?? resolvedMinCount + 20;

                if (resolvedMinCount < 0)
                {
                    return CountError("'minCount' cannot be negative");
                }

                if (resolvedMaxCount < 0)
                {
                    return CountError("'maxCount' cannot be negative");
                }

                if (resolvedMinCount > resolvedMaxCount)
                {
                    return CountError("'minCount' cannot be greater than 'maxCount'");
                }

                return (resolvedMinCount, resolvedMaxCount);
            }

            return count.Match(
                onUnspecified: () => FromUnspecified(),
                onExact: count => FromExact(count),
                onBounded: (minCount, maxCount) => FromBounded(minCount, maxCount));
        }

        private static IGen<IReadOnlyCollection<T>> Error(string message) =>
            Gen.Advanced.Error<IReadOnlyCollection<T>>(nameof(SetGen<T>), message);

        private static IGen<IReadOnlyCollection<T>> CountLimitError() =>
            Error($"Count limit exceeded. This is a built-in safety mechanism to prevent hanging tests. If generating a list with over 1000 elements was intended, relax this constraint by calling {nameof(ISetGen<T>)}.{nameof(ISetGen<T>.DisableCountLimitUnsafe)}().");
    }
}