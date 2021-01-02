using GalaxyCheck.Abstractions;
using GalaxyCheck.ExampleSpaces;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Gens
{
    public delegate int NextIntFunc(int min, int max);

    public delegate T StatefulGenFunc<T>(NextIntFunc useNextInt);

    public static class PrimitiveGenBuilder
    {
        public static PrimitiveGenBuilder<T> Create<T>(
            StatefulGenFunc<T> generate,
            ShrinkFunc<T> shrink,
            MeasureFunc<T> measure) => new PrimitiveGenBuilder<T>(new PrimitiveGen<T>(generate, shrink, measure));

        private class PrimitiveGen<T> : IGen<T>
        {
            private StatefulGenFunc<T> _generate;
            private ShrinkFunc<T> _shrink;
            private MeasureFunc<T> _measure;

            public PrimitiveGen(StatefulGenFunc<T> generate, ShrinkFunc<T> shrink, MeasureFunc<T> measure)
            {
                _generate = generate;
                _shrink = shrink;
                _measure = measure;
            }

            public IEnumerable<GenIteration<T>> Run(IRng rng)
            {
                NextIntFunc useNextInt = (min, max) =>
                {
                    var value = rng.Value(min, max);
                    rng = rng.Next();
                    return value;
                };

                do
                {
                    var initRng = rng;

                    var exampleSpace = ExampleSpace.Unfold(_generate(useNextInt), _shrink, _measure);

                    yield return new GenInstance<T>(exampleSpace.Traverse().First().Value);
                } while (true);
            }
        }
    }

    public class PrimitiveGenBuilder<T> : BaseGenBuilder<T>, IGenBuilder<T>
    {
        public PrimitiveGenBuilder(IGen<T> innerGen) : base(innerGen)
        {
        }
    }
}
