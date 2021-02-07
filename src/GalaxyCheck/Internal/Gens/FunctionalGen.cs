using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Sizing;
using System.Collections.Generic;

namespace GalaxyCheck.Internal.Gens
{
    public delegate IEnumerable<IGenIteration<T>> GenFunc<T>(IRng rng, Size size);

    public class FunctionalGen<T> : BaseGen<T>, IGen<T>
    {
        private readonly GenFunc<T> _func;

        public FunctionalGen(GenFunc<T> func)
        {
            _func = func;
        }

        protected override IEnumerable<IGenIteration<T>> Run(IRng rng, Size size) => _func(rng, size);
    }
}
