using GalaxyCheck.Internal.GenIterations;
using System;
using System.Collections.Generic;

namespace GalaxyCheck.Internal.Gens
{
    internal class LazyGen<T> : BaseGen<T>
    {
        private readonly Lazy<IGen<T>> _lazy;

        public LazyGen(Func<IGen<T>> gen)
        {
            _lazy = new Lazy<IGen<T>>(gen);
        }

        protected override IEnumerable<IGenIteration<T>> Run(GenParameters parameters)
        {
            return _lazy.Value.Advanced.Run(parameters);
        }
    }
}
