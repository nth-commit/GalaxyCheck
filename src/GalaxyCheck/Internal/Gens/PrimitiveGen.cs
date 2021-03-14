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

        public PrimitiveGen(StatefulGenFunc<T> generate)
        {
            _generate = generate;
        }

        protected override IEnumerable<IGenIteration<T>> Run(GenParameters parameters)
        {
            NextIntFunc useNextInt = (min, max) =>
            {
                var value = parameters.Rng.Value(min, max);
                parameters = parameters.With(rng: parameters.Rng.Next());
                return value;
            };

            do
            {
                var initialParameters = parameters;

                var exampleSpace = ExampleSpaceFactory.Singleton(_generate(useNextInt, parameters.Size));

                yield return GenIterationFactory.Instance(initialParameters, parameters, exampleSpace);
            } while (true);
        }
    }
}