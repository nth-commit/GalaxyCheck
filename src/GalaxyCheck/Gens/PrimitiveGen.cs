using GalaxyCheck.Abstractions;
using GalaxyCheck.ExampleSpaces;
using System.Collections.Generic;

namespace GalaxyCheck.Gens
{
    public delegate int NextIntFunc(int min, int max);

    public delegate T StatefulGenFunc<T>(NextIntFunc useNextInt, ISize size);

    public class PrimitiveGen<T> : IGen<T>
    {
        private readonly StatefulGenFunc<T> _generate;
        private readonly ShrinkFunc<T> _shrink;
        private readonly MeasureFunc<T> _measure;

        public PrimitiveGen(StatefulGenFunc<T> generate, ShrinkFunc<T> shrink, MeasureFunc<T> measure)
        {
            _generate = generate;
            _shrink = shrink;
            _measure = measure;
        }

        public IEnumerable<GenIteration<T>> Run(IRng rng, ISize size)
        {
            NextIntFunc useNextInt = (min, max) =>
            {
                var value = rng.Value(min, max);
                rng = rng.Next();
                return value;
            };

            do
            {
                var initialRng = rng;

                var exampleSpace = ExampleSpace.Unfold(_generate(useNextInt, size), _shrink, _measure);

                yield return new GenInstance<T>(initialRng, rng, exampleSpace);
            } while (true);
        }
    }
}
