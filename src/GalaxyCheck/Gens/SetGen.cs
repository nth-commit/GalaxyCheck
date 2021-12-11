namespace GalaxyCheck
{
    using GalaxyCheck.Gens;
    using GalaxyCheck.Gens.Internal;

    public static partial class Gen
    {
        public static ISetGen<T> Set<T>(IGen<T> elementGen) => new SetGen<T>(
            ElementGen: elementGen,
            Count: RangeIntention.Unspecified());
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
    using GalaxyCheck.ExampleSpaces;
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Internal.Iterations;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using GalaxyCheck.Internal;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

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
        IGen<T> ElementGen,
        RangeIntention Count) : GenProvider<IReadOnlyCollection<T>>, ISetGen<T>
    {
        public ISetGen<T> WithCount(int count) => this with { Count = RangeIntention.Exact(count) };

        public ISetGen<T> WithCountGreaterThanEqual(int minCount) => this with { Count = Count.WithMinimum(minCount) };

        public ISetGen<T> WithCountLessThanEqual(int maxCount) => this with { Count = Count.WithMaximum(maxCount) };

        public ISetGen<T> WithCountBias(Gen.Bias bias)
        {
            throw new System.NotImplementedException();
        }

        protected override IGen<IReadOnlyCollection<T>> Get => TryGetCountRange(Count).Match(
            fromLeft: range => range == null
                ? GenSetOfUnspecifiedCount(ElementGen, Gen.Bias.WithSize)
                : GenSetOfSpecifiedCount(ElementGen, range.Value, Gen.Bias.WithSize),
            fromRight: gen => gen);

        private static IGen<IReadOnlyCollection<T>> GenSetOfSpecifiedCount(
            IGen<T> elementGen,
            (int minCount, int maxCount) range,
            Gen.Bias bias)
        {
            var (minCount, maxCount) = range;

            var shrink = ShrinkTowardsCount(minCount);
            var measureCount = MeasureFunc.DistanceFromOrigin(minCount, minCount, maxCount);
            return
                from count in GenCount(minCount, maxCount, bias)
                from set in GenSetOfCount(elementGen, count, minCount, shrink, measureCount)
                select set;
        }

        private static IGen<int> GenCount(int minCount, int maxCount, Gen.Bias bias) =>
            Gen.Int32().GreaterThanEqual(minCount).LessThanEqual(maxCount).WithBias(bias).NoShrink();

        private static IGen<IReadOnlyCollection<T>> GenSetOfCount(
            IGen<T> elementGen,
            int count,
            int minCount,
            ShrinkFunc<List<IExampleSpace<T>>> shrink,
            MeasureFunc<int> measureCount)
        {
            IEnumerable<IGenIteration<IReadOnlyCollection<T>>> Run(GenParameters parameters)
            {
                var nextParameters = parameters;
                var values = ImmutableHashSet<T>.Empty;
                var instances = ImmutableList<IGenInstance<T>>.Empty;

                while (instances.Count < count)
                {
                    var elementIterationEnumerator = elementGen
                        .Where(value => values.Contains(value) == false)
                        .Advanced.Run(nextParameters).GetEnumerator();

                    while (true)
                    {
                        if (!elementIterationEnumerator.MoveNext())
                        {
                            throw new Exception("Fatal: Element generator exhausted");
                        }

                        var either = elementIterationEnumerator.Current.ToEither<T, ImmutableList<T>>();

                        if (either.IsLeft(out IGenInstance<T> instance))
                        {
                            instances = instances.Add(instance);
                            values = values.Add(instance.ExampleSpace.Current.Value);
                            nextParameters = instance.NextParameters;
                            break;
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
                }

                var exampleSpace = ExampleSpaceFactory
                    .Merge(
                        instances.Select(instance => instance.ExampleSpace).ToList(),
                        values => new HashSet<T>(values),
                        shrink,
                        exampleSpaces => exampleSpaces.Sum(exs => exs.Current.Distance) + measureCount(exampleSpaces.Count),
                        enableSmallestExampleSpacesOptimization: true)
                    .Filter(set => set.Count >= minCount);

                yield return GenIterationFactory.Instance(parameters, nextParameters, exampleSpace!);
            }

            return new FunctionalGen<IReadOnlyCollection<T>>(Run).Repeat();
        }

        private static IGen<IReadOnlyCollection<T>> GenSetOfUnspecifiedCount(
            IGen<T> elementGen,
            Gen.Bias bias)
        {
            var minAttempts = 0;
            var maxAttempts = 100;

            var shrink = ShrinkTowardsCount(0);
            var measureCount = MeasureFunc.DistanceFromOrigin(minAttempts, minAttempts, maxAttempts);

            return
                from attempts in Gen.Int32().Between(minAttempts, maxAttempts).WithBias(bias).NoShrink()
                from set in GenSetOfAttempts(elementGen, attempts, shrink, measureCount)
                select set;
        }

        private static IGen<IReadOnlyCollection<T>> GenSetOfAttempts(
            IGen<T> elementGen,
            int attempts,
            ShrinkFunc<List<IExampleSpace<T>>> shrink,
            MeasureFunc<int> measureCount)
        {
            IEnumerable<IGenIteration<IReadOnlyCollection<T>>> Run(GenParameters parameters)
            {
                var nextParameters = parameters;
                var values = ImmutableHashSet<T>.Empty;
                var instances = ImmutableList<IGenInstance<T>>.Empty;
                var numberOfAttempts = 0;

                while (numberOfAttempts < attempts)
                {
                    var elementIterationEnumerator = elementGen
                        .Where(value => values.Contains(value) == false)
                        .Advanced.Run(nextParameters).GetEnumerator();

                    while (numberOfAttempts < attempts)
                    {
                        if (!elementIterationEnumerator.MoveNext())
                        {
                            throw new Exception("Fatal: Element generator exhausted");
                        }

                        var either = elementIterationEnumerator.Current.ToEither<T, ImmutableList<T>>();

                        if (either.IsLeft(out IGenInstance<T> instance))
                        {
                            numberOfAttempts++;
                            instances = instances.Add(instance);
                            values = values.Add(instance.ExampleSpace.Current.Value);
                            nextParameters = instance.NextParameters;
                            break;
                        }
                        else if (either.IsRight(out IGenIteration<ImmutableList<T>> right))
                        {
                            numberOfAttempts++;
                            yield return right;
                        }
                        else
                        {
                            throw new Exception("Fatal: Unhandled branch");
                        }
                    }
                }

                var exampleSpace = ExampleSpaceFactory.Merge(
                    instances.Select(instance => instance.ExampleSpace).ToList(),
                    values => new HashSet<T>(values),
                    shrink,
                    exampleSpaces => exampleSpaces.Sum(exs => exs.Current.Distance) + measureCount(exampleSpaces.Count),
                    enableSmallestExampleSpacesOptimization: true);

                yield return GenIterationFactory.Instance(parameters, nextParameters, exampleSpace!);
            }

            return new FunctionalGen<IReadOnlyCollection<T>>(Run).Repeat();
        }

        private static ShrinkFunc<List<IExampleSpace<T>>> ShrinkTowardsCount(int count)
        {
            // Order of sets should not be important, so don't provide a variable order function. This cuts down the
            // number of shrinks.
            return ShrinkFunc.TowardsCountOptimized<IExampleSpace<T>, decimal>(count, exampleSpace => 0);
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
    }
}
