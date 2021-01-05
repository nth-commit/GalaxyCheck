using GalaxyCheck.Abstractions;
using System;
using System.Collections.Generic;

namespace GalaxyCheck.Gens
{
    public class DelayedGen<T> : IGen<T>
    {
        private readonly Func<IGen<T>> _create;

        public DelayedGen(Func<IGen<T>> create)
        {
            _create = create;
        }

        public IEnumerable<GenIteration<T>> Run(IRng rng, ISize size)
        {
            var gen = _create();
            return gen.Run(rng, size);
        }
    }
}
