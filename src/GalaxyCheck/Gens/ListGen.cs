namespace GalaxyCheck
{
    using GalaxyCheck.Gens;

    public static partial class Gen
    {
        /// <summary>
        /// Creates a generator that produces lists, the elements of which are produced by the given generator. By
        /// default, the generator produces lists ranging from count 0 to 20 - but this can be configured using the
        /// builder methods on <see cref="IListGen{T}"/>.
        /// </summary>
        /// <param name="elementGen">The generator used to produce the elements of the list.</param>
        /// <returns>The new generator.</returns>
        public static IListGen<T> List<T>(IGen<T> elementGen) => new ListGen<T>(elementGen);
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Creates a generator that produces lists, the elements of which are produced by the given generator. By
        /// default, the generator produces lists ranging from count 0 to 20 - but this can be configured using the
        /// builder methods on <see cref="IListGen{T}"/>.
        /// </summary>
        /// <param name="elementGen">The generator used to produce the elements of the list.</param>
        /// <returns>The new generator.</returns>
        public static IListGen<T> ListOf<T>(this IGen<T> gen) => Gen.List(gen);

        /// <summary>
        /// Constrains the generator so that it only produces lists with counts within the supplied range (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IListGen<T> WithCountBetween<T>(this IListGen<T> gen, int x, int y)
        {
            var minCount = x > y ? y : x;
            var maxCount = x > y ? x : y;
            return gen.WithCountGreaterThanEqual(minCount).WithCountLessThanEqual(maxCount);
        }

        /// <summary>
        /// Constrains the generator so that it only produces lists with greater than the given count.
        /// </summary>
        /// <param name="exclusiveMinCount">The minimum count that generated lists should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IListGen<T> WithCountGreaterThan<T>(this IListGen<T> gen, int exclusiveMinCount) =>
            gen.WithCountGreaterThanEqual(exclusiveMinCount + 1);

        /// <summary>
        /// Constrains the generator so that it only produces lists less than the given count.
        /// </summary>
        /// <param name="exclusiveMaxCount">The maximum count that generated lists should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IListGen<T> WithCountLessThan<T>(this IListGen<T> gen, int exclusiveMaxCount) =>
            gen.WithCountLessThanEqual(exclusiveMaxCount - 1);
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Internal.Iterations;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using GalaxyCheck.ExampleSpaces;
    using GalaxyCheck.Internal;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public interface IListGen<T> : IGen<IReadOnlyList<T>>
    {
        /// <summary>
        /// Constrains the generator so that it only produces lists with the given count.
        /// </summary>
        /// <param name="count">The count to constrain generated lists to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IListGen<T> WithCount(int count);

        /// <summary>
        /// Constrains the generator so that it only produces lists with at least the given count.
        /// </summary>
        /// <param name="minCount">The minimum count that generated lists should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IListGen<T> WithCountGreaterThanEqual(int minCount);

        /// <summary>
        /// Constrains the generator so that it only produces lists with at most the given count.
        /// </summary>
        /// <param name="maxCount">The maximum count that generated lists should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IListGen<T> WithCountLessThanEqual(int maxCount);

        /// <summary>
        /// Modifies how the generator biases counts of produced lists with respect to the size parameter.
        /// </summary>
        /// <returns>A new generator with the biasing effect applied.</returns>
        IListGen<T> WithCountBias(Gen.Bias bias);

        /// <summary>
        /// <para>
        /// By default, trying to generate a list with a count greater than 1000 is disabled. This is a built-in
        /// safety mechanism to prevent confusing test failures or hangs. Huge lists can potentially blow out the
        /// performance footprint of a test to unattainable levels, because the generation performance for lists is
        /// O(n).
        /// </para>
        /// <para>
        /// It's quite easy to accidentally create a list generator for huge lists. For example, the following code
        /// might otherwise attempt to generate a list with count int.MaxValue:
        /// <code>
        /// Gen.Int32().GreaterThanEqual(0).SelectMany(count => Gen.Int32().ListOf().WithCount(count));
        /// </code>
        /// </para>
        /// </summary>
        /// <returns></returns>
        IListGen<T> DisableCountLimitUnsafe();
    }

    internal abstract record ListGenCountConfig
    {
        public record Specific(int Count) : ListGenCountConfig;

        public record Ranged(int? MinCount, int? MaxCount) : ListGenCountConfig;
    }

    internal record ListGen<T>(
        IGen<T> ElementGen,
        ListGenCountConfig? CountConfig = null,
        Gen.Bias? Bias = null,
        bool? EnableCountLimit = null) : GenProvider<IReadOnlyList<T>>, IListGen<T>
    {
        public IListGen<T> WithCount(int count) => this with { CountConfig = new ListGenCountConfig.Specific(count) };

        public IListGen<T> WithCountGreaterThanEqual(int minCount) =>
            this with
            {
                CountConfig = new ListGenCountConfig.Ranged(
                    MinCount: minCount,
                    MaxCount: CountConfig is ListGenCountConfig.Ranged rangedCountConfig
                        ? rangedCountConfig.MaxCount
                        : null)
            };

        public IListGen<T> WithCountLessThanEqual(int maxCount) =>
            this with
            {
                CountConfig = new ListGenCountConfig.Ranged(
                    MinCount: CountConfig is ListGenCountConfig.Ranged rangedCountConfig
                        ? rangedCountConfig.MinCount
                        : null,
                    MaxCount: maxCount)
            };

        public IListGen<T> WithCountBias(Gen.Bias bias) => this with { Bias = bias };

        public IListGen<T> DisableCountLimitUnsafe() => this with { EnableCountLimit = false };

        protected override IGen<IReadOnlyList<T>> Get
        {
            get
            {
                var (minCount, maxCount, countGen) = CountGen();
                var shrink = ShrinkTowardsCount(minCount);

                return
                    from count in countGen
                    from list in GenOfCount(count, minCount, maxCount, shrink)
                    select list;
            }
        }

        private (int minCount, int maxCount, IGen<int> gen) CountGen()
        {
            static (int minCount, int maxCount, IGen<int> gen) CountError(string message) =>
                (-1, -1, Gen.Advanced.Error<int>(nameof(ListGen<T>), message));

            static (int minCount, int maxCount, IGen<int> gen) CountLimitError() =>
                CountError($"Count limit exceeded. This is a built-in safety mechanism to prevent hanging tests. If generating a list with over 1000 elements was intended, relax this constraint by calling {nameof(IListGen<T>)}.{nameof(IListGen<T>.DisableCountLimitUnsafe)}().");

            static (int minCount, int maxCount, IGen<int> gen) SpecificCountGen(int count, bool enableCountLimit)
            {
                if (count < 0)
                {
                    return CountError("'count' cannot be negative");
                }

                if (enableCountLimit && count > 1000)
                {
                    return CountLimitError();
                }

                return (count, count, Gen.Constant(count));
            }

            static (int minCount, int maxCount, IGen<int> gen) RangedCountGen(int? minCount, int? maxCount, Gen.Bias? bias, bool enableCountLimit)
            {
                var resolvedMinCount = minCount ?? 0;
                var resolvedMaxCount = maxCount ?? resolvedMinCount + 20;
                var resolvedBias = bias ?? Gen.Bias.WithSize;

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

                if (enableCountLimit && (resolvedMinCount > 1000 || resolvedMaxCount > 1000))
                {
                    return CountLimitError();
                }

                // TODO: If minCount == maxCount, defer to SpecificCountGen

                var gen = Gen
                    .Int32()
                    .GreaterThanEqual(resolvedMinCount)
                    .LessThanEqual(resolvedMaxCount)
                    .WithBias(resolvedBias)
                    .NoShrink();

                return (resolvedMinCount, resolvedMaxCount, gen);
            }

            return CountConfig switch
            {
                ListGenCountConfig.Specific specificCountConfig => SpecificCountGen(
                    count: specificCountConfig.Count,
                    enableCountLimit: EnableCountLimit ?? true),
                ListGenCountConfig.Ranged rangedCountConfig => RangedCountGen(
                    minCount: rangedCountConfig.MinCount,
                    maxCount: rangedCountConfig.MaxCount,
                    bias: Bias,
                    enableCountLimit: EnableCountLimit ?? true),
                null => RangedCountGen(
                    minCount: null,
                    maxCount: null,
                    bias: Bias,
                    enableCountLimit: EnableCountLimit ?? true),
                _ => throw new NotSupportedException()
            };
        }

        private IGen<ImmutableList<T>> GenOfCount(
            int count,
            int minCount,
            int maxCount,
            ShrinkFunc<List<IExampleSpace<T>>> shrink)
        {
            IEnumerable<IGenIteration<ImmutableList<T>>> Run(GenParameters parameters)
            {
                var instances = ImmutableList<IGenInstance<T>>.Empty;

                var elementIterationEnumerator = ElementGen.Advanced.Run(parameters).GetEnumerator();

                while (instances.Count < count)
                {
                    if (!elementIterationEnumerator.MoveNext())
                    {
                        throw new Exception("Fatal: Element generator exhausted");
                    }

                    var either = elementIterationEnumerator.Current.ToEither<T, ImmutableList<T>>();

                    if (either.IsLeft(out IGenInstance<T> instance))
                    {
                        instances = instances.Add(instance);
                    }
                    else if (either.IsRight(out IGenIteration<ImmutableList<T>> right))
                    {
                        yield return right;
                    }
                    else
                    {
                        throw new Exception("Fatal: Unhandled branch");
                    }
                }

                var nextParameters = instances.Any() ? instances.Last().NextParameters : parameters;

                var measureListCount = MeasureFunc.DistanceFromOrigin(minCount, minCount, maxCount);

                var exampleSpace = ExampleSpaceFactory.Merge(
                    instances.Select(instance => instance.ExampleSpace).ToList(),
                    values => values.ToImmutableList(),
                    shrink,
                    exampleSpaces => exampleSpaces.Sum(exs => exs.Current.Distance) + measureListCount(exampleSpaces.Count));

                yield return GenIterationFactory.Instance(parameters, nextParameters, exampleSpace);
            }

            return new FunctionalGen<ImmutableList<T>>(Run).Repeat();
        }

        private static ShrinkFunc<List<IExampleSpace<T>>> ShrinkTowardsCount(int count)
        {
            // If the value type is a collection, that is, this generator is building a "collection of collections",
            // it is "less complex" to order the inner collections by descending count. It also lets us find the
            // minimal shrink a lot more efficiently in some examples,
            // e.g. https://github.com/jlink/shrinking-challenge/blob/main/challenges/large_union_list.md

            return ShrinkFunc.TowardsCountOptimized<IExampleSpace<T>, decimal>(count, exampleSpace =>
            {
                return -exampleSpace.Current.Distance;
            });
        }

        private static IGen<ImmutableList<T>> Error(string message) => Gen.Advanced.Error<ImmutableList<T>>(nameof(ListGen<T>), message);
    }
}