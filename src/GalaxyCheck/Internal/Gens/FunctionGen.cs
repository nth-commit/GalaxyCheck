using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Sizing;
using System.Collections.Generic;

namespace GalaxyCheck.Internal.Gens
{
    public delegate IEnumerable<GenIteration<T>> GenFunc<T>(IRng rng, Size size);

    public class FunctionGen<T> : BaseGen<T>, IGen<T>
    {
        private readonly GenFunc<T> _func;

        public FunctionGen(GenFunc<T> func)
        {
            _func = func;
        }

        protected override IEnumerable<GenIteration<T>> Run(IRng rng, Size size) => _func(rng, size);
    }
}
