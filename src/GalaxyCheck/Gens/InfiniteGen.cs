using GalaxyCheck.Gens;
using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using GalaxyCheck.Internal.Sizing;
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

                var exampleSpace = CreateInfiniteEnumerableSpace(_elementGen, forkedRng, size);

                yield return new GenInstance<IEnumerable<T>>(initialRng, size, nextRng, size, exampleSpace);

                rng = rng.Next();
            } while (true);
        }

        private static ExampleSpace<IEnumerable<T>> CreateInfiniteEnumerableSpace(
            IGen<T> elementGen,
            IRng rng,
            Size size)
        {
            var enumerable = CreateInfiniteEnumerable(elementGen, rng, size);
            return ExampleSpace.Singleton(enumerable);
        }

        private static IEnumerable<T> CreateInfiniteEnumerable(
            IGen<T> elementGen,
            IRng rng,
            Size size)
        {
            var source = elementGen.Advanced
                .Run(rng, size)
                .OfType<GenInstance<T>>()
                .Select(iteration => iteration.ExampleSpace);

            return source.Select(es => es.Current.Value);
        }
    }

}
