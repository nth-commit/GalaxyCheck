using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Internal.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.Sizing;
using System.Collections.Generic;

namespace GalaxyCheck
{
    using GalaxyCheck.Gens;

    public static partial class Gen
    {
        public static partial class Advanced
        {
            public static IGen<T> Create<T>(StatefulGenFunc<T> generate) => new PrimitiveGen<T>(generate);
        }
    }
}

namespace GalaxyCheck.Gens
{
    public delegate int NextIntFunc(int min, int max);

    public delegate T StatefulGenFunc<T>(NextIntFunc useNextInt, Size size);

    internal class PrimitiveGen<T> : BaseGen<T>, IGen<T>
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