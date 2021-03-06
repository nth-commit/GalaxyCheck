﻿using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using System;
using System.Collections.Generic;

namespace GalaxyCheck.Gens.Internal
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
