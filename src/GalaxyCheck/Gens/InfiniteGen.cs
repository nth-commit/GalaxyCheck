using GalaxyCheck.Gens;
using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using GalaxyCheck.Internal.Sizing;
using GalaxyCheck.Internal.Utility;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        public static IInfiniteGen<T> Infinite<T>(IGen<T> elementGen) => new InfiniteGen<T>(elementGen);

        public static IInfiniteGen<T> InfiniteOf<T>(this IGen<T> elementGen) => Infinite(elementGen);
    }
}

namespace GalaxyCheck.Gens
{
    public interface IInfiniteGen<T> : IGen<IEnumerable<T>> { }

    public class InfiniteGen<T> : BaseGen<IEnumerable<T>>, IInfiniteGen<T>
    {
        private readonly IGen<T> _elementGen;
        private readonly int? _iterationLimit = 100;

        public InfiniteGen(IGen<T> elementGen)
        {
            _elementGen = elementGen;
        }

        protected override IEnumerable<GenIteration<IEnumerable<T>>> Run(IRng rng, Size size)
        {
            do
            {
                var initialRng = rng;
                var nextRng = initialRng.Next();
                var forkedRng = initialRng.Fork();

                var exampleSpace = CreateInfiniteEnumerableSpace(_elementGen, forkedRng, size, _iterationLimit);

                yield return new GenInstance<IEnumerable<T>>(initialRng, size, nextRng, size, exampleSpace);

                rng = rng.Next();
            } while (true);
        }

        private static ExampleSpace<IEnumerable<T>> CreateInfiniteEnumerableSpace(
            IGen<T> elementGen,
            IRng rng,
            Size size,
            int? iterationLimit)
        {
            var enumerable = CreateInfiniteEnumerable(elementGen, rng, size, iterationLimit);

            var rootExample = new Example<IEnumerable<T>>(
                ExampleId.Primitive(rng.Seed),
                enumerable.Select(x => x.Current.Value),
                100);

            return ExampleSpace.Delay(
                rootExample,
                () =>
                {
                    if (enumerable.MaxIterations == 0)
                    {
                        return Enumerable.Empty<ExampleSpace<IEnumerable<T>>>();
                    }

                    var exampleSpaces = enumerable.Take(enumerable.MaxIterations).ToList();

                    var rootExampleExplored = ExampleSpace.Merge(
                        exampleSpaces,
                        xs => xs,
                        ShrinkTowardsLength(1),
                        (_) => 0);

                    return rootExampleExplored.Subspace.Select(es => es.Map(xs => ThrowOnLimit(xs.Repeat(), iterationLimit)));
                });
        }

        private static SpyEnumerable<ExampleSpace<T>> CreateInfiniteEnumerable(
            IGen<T> elementGen,
            IRng rng,
            Size size,
            int? iterationLimit)
        {
            var source = elementGen.Advanced
                .Run(rng, size)
                .OfType<GenInstance<T>>()
                .Select(iteration => iteration.ExampleSpace);

            return new SpyEnumerable<ExampleSpace<T>>(ThrowOnLimit(source, iterationLimit));
        }

        private static ShrinkFunc<List<ExampleSpace<T>>> ShrinkTowardsLength(int length)
        {
            // If the value type is a collection, that is, this generator is building a "collection of collections",
            // it is "less complex" to order the inner collections by descending length. It also lets us find the
            // minimal shrink a lot more efficiently in some examples,
            // e.g. https://github.com/jlink/shrinking-challenge/blob/main/challenges/large_union_list.md

            return ShrinkFunc.TowardsCount2<ExampleSpace<T>, decimal>(length, exampleSpace =>
            {
                return -exampleSpace.Current.Distance;
            });
        }

        private static IEnumerable<U> ThrowOnLimit<U>(IEnumerable<U> source, int? iterationLimit) =>
            iterationLimit.HasValue
                ? source.Select((x, i) =>
                {
                    if (i > iterationLimit.Value)
                    {
                        throw new System.Exception("Iteration limit: TODO, test and improve message");
                    }

                    return x;
                })
                : source;

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
