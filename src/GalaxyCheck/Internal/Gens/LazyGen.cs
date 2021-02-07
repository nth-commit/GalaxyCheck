using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Sizing;
using System;
using System.Collections.Generic;

namespace GalaxyCheck.Internal.Gens
{
    public class LazyGen<T> : BaseGen<T>
    {
        private readonly Lazy<IGen<T>> _lazy;

        public LazyGen(Func<IGen<T>> gen)
        {
            _lazy = new Lazy<IGen<T>>(gen);
        }

        protected override IEnumerable<IGenIteration<T>> Run(IRng rng, Size size)
        {
            return _lazy.Value.Advanced.Run(rng, size);
        }
    }
}
