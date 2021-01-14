using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Sizing;
using System.Collections.Generic;

namespace GalaxyCheck.Internal.Gens
{
    public delegate int NextIntFunc(int min, int max);

    public delegate T StatefulGenFunc<T>(NextIntFunc useNextInt, Size size);

    public class PrimitiveGen<T> : BaseGen<T>, IGen<T>
    {
        private readonly StatefulGenFunc<T> _generate;
        private readonly ShrinkFunc<T> _shrink;
        private readonly MeasureFunc<T> _measure;
        private readonly IdentifyFunc<T> _identify;

        public PrimitiveGen(
            StatefulGenFunc<T> generate,
            ShrinkFunc<T> shrink,
            MeasureFunc<T> measure,
            IdentifyFunc<T> identify)
        {
            _generate = generate;
            _shrink = shrink;
            _measure = measure;
            _identify = identify;
        }

        protected override IEnumerable<GenIteration<T>> Run(IRng rng, Size size)
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

                var exampleSpace = ExampleSpace.Unfold(_generate(useNextInt, size), _shrink, _measure, _identify);

                yield return new GenInstance<T>(initialRng, size, rng, size, exampleSpace);
            } while (true);
        }
    }
}
