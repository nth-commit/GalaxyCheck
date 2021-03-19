﻿using GalaxyCheck.Internal.GenIterations;
using System.Collections.Generic;

namespace GalaxyCheck.Internal.Gens
{
    internal delegate IEnumerable<IGenIteration<T>> GenFunc<T>(GenParameters parameters);

    internal class FunctionalGen<T> : BaseGen<T>, IGen<T>
    {
        private readonly GenFunc<T> _func;

        public FunctionalGen(GenFunc<T> func)
        {
            _func = func;
        }

        protected override IEnumerable<IGenIteration<T>> Run(GenParameters parameters) => _func(parameters);
    }
}
