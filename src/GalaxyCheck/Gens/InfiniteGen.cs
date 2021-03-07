using GalaxyCheck.Gens;
using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using GalaxyCheck.Internal.Sizing;
using GalaxyCheck.Internal.Utility;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        /// <summary>
        /// Creates a generator that produces infinite enumerables, the elements of which are produced by the given
        /// generator. Infinite enumerables are supported by this generator, but by default there is a limit of 1000
        /// elements in place - and the iterator will throw if that limit is exceeded. This is a pragmatic limit to
        /// avert confusion of cases where the consuming code trys to enumerate an infinite enumerable (and
        /// consequently hangs forever). This can be configured with the builder methods
        /// <see cref="IInfiniteGen{T}.WithIterationLimit(int)"/> or
        /// <see cref="IInfiniteGen{T}.WithoutIterationLimit"/>.
        /// </summary>
        /// <param name="elementGen">The generator used to produce the elements of the list.</param>
        /// <returns>The new generator.</returns>
        public static IInfiniteGen<T> Infinite<T>(IGen<T> elementGen) => new InfiniteGen<T>(elementGen);

        /// <summary>
        /// Creates a generator that produces infinite enumerables, the elements of which are produced by the given
        /// generator. Infinite enumerables are supported by this generator, but by default there is a limit of 1000
        /// elements in place - and the iterator will throw if that limit is exceeded. This is a pragmatic limit to
        /// avert confusion of cases where the consuming code trys to enumerate an infinite enumerable (and
        /// consequently hangs forever). This can be configured with the builder methods
        /// <see cref="IInfiniteGen{T}.WithIterationLimit(int)"/> or
        /// <see cref="IInfiniteGen{T}.WithoutIterationLimit"/>.
        /// </summary>
        /// <param name="elementGen">The generator used to produce the elements of the list.</param>
        /// <returns>The new generator.</returns>
        public static IInfiniteGen<T> InfiniteOf<T>(this IGen<T> elementGen) => Infinite(elementGen);
    }
}

namespace GalaxyCheck.Gens
{
    public interface IInfiniteGen<T> : IGen<IEnumerable<T>>
    {
        /// <summary>
        /// Modifies the iteration limit of the enumerable. Unless you're hitting the limit then it's not recommended
        /// to customize this. You will know you're hitting the limit because the generator will loudly
        /// tell you that you are, by crashing.
        /// </summary>
        /// <param name="limit"></param>
        /// <returns>A new generator with the modified iteration limit.</returns>
        IInfiniteGen<T> WithIterationLimit(int limit);

        /// <summary>
        /// Completely disables the iteration limit of the enumerable. It's not recommended to customize this, and is
        /// only useful if you want to generate enumerables that you effectively want to enumerate forever. The
        /// recommended API for modifying limits is <see cref="WithIterationLimit(int)"/>.
        /// <returns>A new generator with the modified iteration limit.</returns>
        IInfiniteGen<T> WithoutIterationLimit();
    }

    public class InfiniteGen<T> : BaseGen<IEnumerable<T>>, IInfiniteGen<T>
    {
        private readonly IGen<T> _elementGen;
        private readonly int? _iterationLimit;

        private InfiniteGen(IGen<T> elementGen, int? iterationLimit)
        {
            _elementGen = elementGen;
            _iterationLimit = iterationLimit;
        }

        public InfiniteGen(IGen<T> elementGen)
            : this(elementGen, 1000)
        {
        }

        public IInfiniteGen<T> WithIterationLimit(int limit) => new InfiniteGen<T>(_elementGen, limit);

        public IInfiniteGen<T> WithoutIterationLimit() => new InfiniteGen<T>(_elementGen, null);

        protected override IEnumerable<IGenIteration<IEnumerable<T>>> Run(GenParameters parameters)
        {
            var rng = parameters.Rng;
            do
            {
                var initialRng = rng;
                var nextRng = initialRng.Next();
                var forkedRng = initialRng.Fork();

                var exampleSpace = CreateInfiniteEnumerableSpace(_elementGen, forkedRng, parameters.Size, _iterationLimit);

                yield return GenIterationFactory.Instance(
                    new GenParameters(initialRng, parameters.Size),
                    new GenParameters(nextRng, parameters.Size),
                    exampleSpace);

                rng = rng.Next();
            } while (true);
        }

        private static IExampleSpace<IEnumerable<T>> CreateInfiniteEnumerableSpace(
            IGen<T> elementGen,
            IRng rng,
            Size size,
            int? iterationLimit)
        {
            var enumerable = CreateInfiniteEnumerable(elementGen, rng, size, iterationLimit);

            IExample<IEnumerable<T>> rootExample = new Example<IEnumerable<T>>(
                ExampleId.Primitive(rng.Seed),
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

                    var exampleSpaces = enumerable.Take(enumerable.MaxIterations).ToList();

                    var rootExampleExplored = ExampleSpaceFactory.Merge(
                        exampleSpaces,
                        xs => xs,
                        ShrinkTowardsLength(1),
                        (_) => 0);

                    return rootExampleExplored.Subspace.Select(es => es.Map(xs => ThrowOnLimit(xs.Repeat(), iterationLimit)));
                });
        }

        private static SpyEnumerable<IExampleSpace<T>> CreateInfiniteEnumerable(
            IGen<T> elementGen,
            IRng rng,
            Size size,
            int? iterationLimit)
        {
            var source = elementGen.Advanced
                .Run(new GenParameters(rng, size))
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
            var interfaceIdentifier = "IInfiniteGen";
            var withLimitMethodIdentifier = $"{interfaceIdentifier}.{nameof(IInfiniteGen<object>.WithIterationLimit)}";
            var withoutLimitMethodIdentifier = $"{interfaceIdentifier}.{nameof(IInfiniteGen<object>.WithoutIterationLimit)}";
            var message = $"Infinite enumerable exceeded iteration limit. This is a built-in safety mechanism to prevent hanging tests. Use {withLimitMethodIdentifier} or {withoutLimitMethodIdentifier} to modify this limit.";
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
