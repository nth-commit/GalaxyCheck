﻿using GalaxyCheck.Abstractions;
using System.Collections.Generic;

namespace GalaxyCheck.Gens
{
    public delegate IEnumerable<GenIteration<T>> GenFunc<T>(IRng rng, ISize size);

    public class FunctionGen<T> : BaseGen<T>, IGen<T>
    {
        private readonly GenFunc<T> _func;

        public FunctionGen(GenFunc<T> func)
        {
            _func = func;
        }

        protected override IEnumerable<GenIteration<T>> Run(IRng rng, ISize size) => _func(rng, size);
    }
}