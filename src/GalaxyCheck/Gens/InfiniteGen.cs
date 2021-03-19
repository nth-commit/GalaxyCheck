namespace GalaxyCheck
{
    using GalaxyCheck.Gens;
    using System.Collections.Generic;

    public static partial class Gen
    {
        /// <summary>
        /// Creates a generator that produces infinite enumerables, the elements of which are produced by the given
        /// generator. Infinite enumerables are supported by this generator, but by default there is a limit of 1000
        /// elements in place - and the iterator will throw if that limit is exceeded. This is a pragmatic limit to
        /// avert confusion of cases where the consuming code trys to enumerate an infinite enumerable (and
        /// consequently hangs forever).
        /// </summary>
        /// <param name="elementGen">The generator used to produce the elements of the list.</param>
        /// <param name="iterationLimit">The hard iteration limit that should be applied to the generator.</param>
        /// <returns>The new generator.</returns>
        public static IGen<IEnumerable<T>> Infinite<T>(IGen<T> elementGen, int? iterationLimit = 1000) =>
            new InfiniteGen<T>(elementGen, iterationLimit);
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Creates a generator that produces infinite enumerables, the elements of which are produced by the given
        /// generator. Infinite enumerables are supported by this generator, but by default there is a limit of 1000
        /// elements in place - and the iterator will throw if that limit is exceeded. This is a pragmatic limit to
        /// avert confusion of cases where the consuming code trys to enumerate an infinite enumerable (and
        /// consequently hangs forever).
        /// </summary>
        /// <param name="elementGen">The generator used to produce the elements of the list.</param>
        /// <param name="iterationLimit">The hard iteration limit that should be applied to the generator.</param>
        /// <returns>The new generator.</returns>
        public static IGen<IEnumerable<T>> InfiniteOf<T>(this IGen<T> elementGen, int? iterationLimit = 1000) =>
            Gen.Infinite(elementGen, iterationLimit);
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Internal.ExampleSpaces;
    using GalaxyCheck.Internal.GenIterations;
    using GalaxyCheck.Internal.Gens;
    using GalaxyCheck.Internal.Utility;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    internal class InfiniteGen<T> : BaseGen<IEnumerable<T>>, IGen<IEnumerable<T>>
    {
        private readonly IGen<T> _elementGen;
        private readonly int? _iterationLimit;

        public InfiniteGen(IGen<T> elementGen, int? iterationLimit)
        {
            _elementGen = elementGen;
            _iterationLimit = iterationLimit;
        }

        public InfiniteGen(IGen<T> elementGen)
            : this(elementGen, 1000)
        {
        }

        protected override IEnumerable<IGenIteration<IEnumerable<T>>> Run(GenParameters parameters)
        {
            var rng = parameters.Rng;
            do
            {
                var initialRng = rng;
                var nextRng = initialRng.Next();
                var forkedRng = initialRng.Fork();

                var exampleSpace = CreateInfiniteEnumerableSpace(_elementGen, parameters.With(rng: forkedRng), _iterationLimit);

                yield return GenIterationFactory.Instance(
                    parameters.With(rng: initialRng),
                    parameters.With(rng: nextRng),
                    exampleSpace);

                rng = rng.Next();
            } while (true);
        }

        private static IExampleSpace<IEnumerable<T>> CreateInfiniteEnumerableSpace(
            IGen<T> elementGen,
            GenParameters parameters,
            int? iterationLimit)
        {
            IEnumerable<T> SealEnumerable(IEnumerable<T> source) => ThrowOnLimit(source.Repeat(), iterationLimit);

            var enumerable = CreateInfiniteEnumerable(elementGen, parameters, iterationLimit);

            IExample<IEnumerable<T>> rootExample = new Example<IEnumerable<T>>(
                ExampleId.Primitive(parameters.Rng.Seed),
                enumerable.Select(x => x.Current.Value),
                100);

            return ExampleSpaceFactory.Delay(
                rootExample,
                () =>
                {
                    if (enumerable.MaxIterations == 0)
                    {
                        return Enumerable.Empty<IExampleSpace<IEnumerable<T>>>();
                    }

                    if (enumerable.MaxIterations == 1)
                    {
                        var exampleSpace = enumerable.First().Map(element => SealEnumerable(new[] { element }));
                        return new[] { exampleSpace };
                    }

                    var exampleSpaces = enumerable.Take(enumerable.MaxIterations).ToList();

                    var rootExampleExplored = ExampleSpaceFactory.Merge(
                        exampleSpaces,
                        xs => xs,
                        ShrinkTowardsLength(1),
                        (_) => 0);

                    return rootExampleExplored.Subspace.Select(es => es.Map(SealEnumerable));
                });
        }

        private static SpyEnumerable<IExampleSpace<T>> CreateInfiniteEnumerable(
            IGen<T> elementGen,
            GenParameters parameters,
            int? iterationLimit)
        {
            var source = elementGen.Advanced
                .Run(parameters)
                .WithDiscardCircuitBreaker(iteration => iteration.IsDiscard())
                .Select(iteration => iteration.Match<IGenInstance<T>?>(
                    onInstance: instance => instance,
                    onError: _ => null,
                    onDiscard: _ => null))
                .Where(instance => instance != null)
                .Select(instance => instance!.ExampleSpace);

            return new SpyEnumerable<IExampleSpace<T>>(ThrowOnLimit(source, iterationLimit));
        }

        private static ShrinkFunc<List<IExampleSpace<T>>> ShrinkTowardsLength(int length)
        {
            // If the value type is a collection, that is, this generator is building a "collection of collections",
            // it is "less complex" to order the inner collections by descending length. It also lets us find the
            // minimal shrink a lot more efficiently in some examples,
            // e.g. https://github.com/jlink/shrinking-challenge/blob/main/challenges/large_union_list.md

            return ShrinkFunc.TowardsCountOptimized<IExampleSpace<T>, decimal>(length, exampleSpace =>
            {
                return -exampleSpace.Current.Distance;
            });
        }

        private static IEnumerable<U> ThrowOnLimit<U>(IEnumerable<U> source, int? iterationLimit) =>
            iterationLimit.HasValue
                ? source.Select((x, i) =>
                {
                    var iterationCount = i + 1;
                    if (iterationCount > iterationLimit.Value)
                    {
                        ThrowGenLimitExceeded();
                    }

                    return x;
                })
                : source;

        private static void ThrowGenLimitExceeded()
        {
            var message = "Infinite enumerable exceeded iteration limit. This is a built-in safety mechanism to prevent hanging tests. Relax this constraint by configuring the iterationLimit parameter.";
            throw new Exceptions.GenLimitExceededException(message);
        }

        private class SpyEnumerator<U> : IEnumerator<U>
        {
            private int _currentIterations = 0;
            private int _maxIterations = 0;

            private readonly IEnumerator<U> _inner;

            public SpyEnumerator(IEnumerator<U> inner)
            {
                _inner = inner;
            }

            public int MaxIterations => _maxIterations;

            public U Current => _inner.Current;

            object IEnumerator.Current => Current!;

            public void Dispose()
            {
                _inner.Dispose();
            }

            public bool MoveNext()
            {
                _currentIterations++;

                if (_currentIterations > _maxIterations)
                {
                    _maxIterations = _currentIterations;
                }

                return _inner.MoveNext();
            }

            public void Reset()
            {
                _inner.Reset();
            }
        }

        private class SpyEnumerable<U> : IEnumerable<U>
        {
            private readonly ConcurrentBag<SpyEnumerator<U>> _enumerators = new ConcurrentBag<SpyEnumerator<U>>();
            private readonly IEnumerable<U> _inner;

            public SpyEnumerable(IEnumerable<U> inner)
            {
                _inner = inner;
            }

            public int MaxIterations => _enumerators.Any() ? _enumerators.Max(x => x.MaxIterations) : 0;

            public IEnumerator<U> GetEnumerator()
            {
                var enumerator = new SpyEnumerator<U>(_inner.GetEnumerator());

                _enumerators.Add(enumerator);

                return enumerator;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }

}
