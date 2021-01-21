using GalaxyCheck.Gens;
using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using GalaxyCheck.Internal.Sizing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        /// <summary>
        /// Creates a generator that produces lists, the elements of which are produced by the given generator. By
        /// default, the generator produces lists ranging from length 0 to 20 - but this can be configured using the
        /// builder methods on <see cref="IListGen{T}"/>.
        /// </summary>
        /// <param name="elementGen">The generator used to produce the elements of the list.</param>
        /// <returns>The new generator.</returns>
        public static IListGen<T> List<T>(IGen<T> elementGen) => elementGen.ListOf();
    }

    public static class ListGenExtensions
    {
        /// <summary>
        /// Creates a generator that produces lists, the elements of which are produced by the given generator. By
        /// default, the generator produces lists ranging from length 0 to 20 - but this can be configured using the
        /// builder methods on <see cref="IListGen{T}"/>.
        /// </summary>
        /// <param name="elementGen">The generator used to produce the elements of the list.</param>
        /// <returns>The new generator.</returns>
        public static IListGen<T> ListOf<T>(this IGen<T> gen) => new ListGen<T>(gen);
    }
}

namespace GalaxyCheck.Gens
{
    public interface IListGen<T> : IGen<ImmutableList<T>>
    {
        /// <summary>
        /// Constrains the generator so that it only produces lists with the given length.
        /// </summary>
        /// <param name="length">The length to constrain generated lists to.</param>
        /// <returns>A new generator with the constrain applied.</returns>
        IListGen<T> OfLength(int length);

        /// <summary>
        /// Constrains the generator so that it only produces lists with at least the given length.
        /// </summary>
        /// <param name="minLength">The minimum length that generated lists should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IListGen<T> OfMinimumLength(int minLength);

        /// <summary>
        /// Constrains the generator so that it only produces lists with at most the given length.
        /// </summary>
        /// <param name="maxLength">The maximum length that generated lists should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IListGen<T> OfMaximumLength(int maxLength);

        /// <summary>
        /// Constrains the generator so that it only produces lists with lengths within the supplied range (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IListGen<T> BetweenLengths(int x, int y);

        /// <summary>
        /// Modifies how the generator biases lengths of produced lists with respect to the size parameter.
        /// </summary>
        /// <returns>A new generator with the biasing effect applied.</returns>
        IListGen<T> WithLengthBias(Gen.Bias bias);
    }

    public class ListGen<T> : BaseGen<ImmutableList<T>>, IListGen<T>
    {
        private abstract record ListGenLengthConfig
        {
            private ListGenLengthConfig()
            {
            }

            public record Specific(int Length) : ListGenLengthConfig;

            public record Ranged(int? MinLength, int? MaxLength) : ListGenLengthConfig;
        }

        private record ListGenConfig(ListGenLengthConfig? LengthConfig, Gen.Bias? Bias);

        private readonly ListGenConfig _config;
        private readonly IGen<T> _elementGen;

        private ListGen(ListGenConfig config, IGen<T> elementGen)
        {
            _config = config;
            _elementGen = elementGen;
        }

        public ListGen(IGen<T> elementGen)
            : this(new ListGenConfig(LengthConfig: null, Bias: null), elementGen)
        {
        }

        public IListGen<T> OfLength(int length) =>
            WithPartialConfig(lengthConfig: new ListGenLengthConfig.Specific(length));

        public IListGen<T> OfMinimumLength(int minLength) =>
            WithPartialConfig(lengthConfig: new ListGenLengthConfig.Ranged(
                MinLength: minLength,
                MaxLength: _config.LengthConfig is ListGenLengthConfig.Ranged rangedLengthConfig
                    ? rangedLengthConfig.MaxLength
                    : null));

        public IListGen<T> OfMaximumLength(int maxLength) =>
            WithPartialConfig(lengthConfig: new ListGenLengthConfig.Ranged(
                MinLength: _config.LengthConfig is ListGenLengthConfig.Ranged rangedLengthConfig
                    ? rangedLengthConfig.MinLength
                    : null,
                MaxLength: maxLength));

        public IListGen<T> BetweenLengths(int x, int y) =>
            WithPartialConfig(lengthConfig: new ListGenLengthConfig.Ranged(
                MinLength: x < y ? x : y,
                MaxLength: x > y ? x : y));

        public IListGen<T> WithLengthBias(Gen.Bias bias) => WithPartialConfig(bias: bias);

        private IListGen<T> WithPartialConfig(
            ListGenLengthConfig? lengthConfig = null,
            Gen.Bias? bias = null)
        {
            var newConfig = new ListGenConfig(
                lengthConfig ?? _config.LengthConfig,
                bias ?? _config.Bias);

            return new ListGen<T>(newConfig, _elementGen);
        }

        protected override IEnumerable<GenIteration<ImmutableList<T>>> Run(IRng rng, Size size) =>
            BuildGen(_config, _elementGen).Advanced.Run(rng, size);

        private static IGen<ImmutableList<T>> BuildGen(ListGenConfig config, IGen<T> elementGen)
        {
            var minLength = 0;
            var maxLength = 20;

            if (config.LengthConfig is ListGenLengthConfig.Specific specificLengthConfig)
            {
                if (specificLengthConfig.Length < 0)
                {
                    return Error("'length' cannot be negative");
                }

                minLength = maxLength = specificLengthConfig.Length;
            }
            else if (config.LengthConfig is ListGenLengthConfig.Ranged rangedLengthConfig)
            {
                if (rangedLengthConfig.MinLength < 0)
                {
                    return Error("'minLength' cannot be negative");
                }

                if (rangedLengthConfig.MaxLength < 0)
                {
                    return Error("'maxLength' cannot be negative");
                }

                minLength = rangedLengthConfig.MinLength ?? minLength;
                maxLength = rangedLengthConfig.MaxLength ?? maxLength;
            }

            if (minLength > maxLength)
            {
                return Error("'minLength' cannot be greater than 'maxLength'");
            }

            var lengthGen = Gen
                .Int32()
                .GreaterThanEqual(minLength)
                .LessThanEqual(maxLength)
                .WithBias(config.Bias ?? Gen.Bias.WithSize)
                .NoShrink();

            var shrink = ShrinkTowardsLength(minLength);

            return
                from length in lengthGen
                from list in GenOfLength(length, elementGen, shrink)
                select list;
        }

        private static ShrinkFunc<IEnumerable<ExampleSpace<T>>> ShrinkTowardsLength(int length)
        {
            // TODO: If T implements IEnumerable, order by negative distance.

            // If the value type is a collection, that is, this generator is building a "collection of collections",
            // it is "less complex" to order the inner collections by descending length. It also lets us find the
            // minimal shrink a lot more efficiently in some examples,
            // e.g. https://github.com/jlink/shrinking-challenge/blob/main/challenges/large_union_list.md

            return ShrinkFunc.TowardsCount<ExampleSpace<T>, decimal>(length, exampleSpace =>
            {
                return -exampleSpace.Current.Distance;
            });
        }

        private static IGen<ImmutableList<T>> GenOfLength(
            int length,
            IGen<T> elementGen,
            ShrinkFunc<IEnumerable<ExampleSpace<T>>> shrink)
        {
            IEnumerable<GenIteration<ImmutableList<T>>> Run(IRng rng, Size size)
            {
                var result = ImmutableList<GenInstance<T>>.Empty;

                var elementIterationEnumerator = elementGen.Advanced.Run(rng, size).GetEnumerator();

                while (result.Count < length)
                {
                    if (!elementIterationEnumerator.MoveNext())
                    {
                        throw new Exception("Fatal: Element generator exhausted");
                    }

                    var elementIteration = elementIterationEnumerator.Current;
                    if (elementIteration is GenInstance<T> elementInstance)
                    {
                        result = result.Add(elementInstance);
                    }
                    else
                    {
                        var elementIterationBuilder = GenIterationBuilder.FromIteration(elementIteration);
                        yield return elementIteration.Match<T, GenIteration<ImmutableList<T>>>(
                            onInstance: _ => throw new NotSupportedException(),
                            onDiscard: discard => elementIterationBuilder.ToDiscard<ImmutableList<T>>(),
                            onError: error => elementIterationBuilder.ToError<ImmutableList<T>>(error.GenName, error.Message));
                    }
                }

                var nextRng = result.Any() ? result.Last().NextRng : rng;
                var nextSize = result.Any() ? result.Last().NextSize : size;

                var exampleSpace = ExampleSpace.Merge(
                    result.Select(instance => instance.ExampleSpace),
                    values => values.ToImmutableList(),
                    shrink,
                    exampleSpaces => exampleSpaces.Count());

                yield return new GenInstance<ImmutableList<T>>(
                    rng,
                    size,
                    nextRng,
                    nextSize,
                    exampleSpace);
            }

            return new FunctionGen<ImmutableList<T>>(Run).Repeat() ;
        }

        private static IGen<ImmutableList<T>> Error(string message) => new ErrorGen<ImmutableList<T>>(nameof(ListGen<T>), message);
    }
}