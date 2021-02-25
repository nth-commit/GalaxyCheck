using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Sizing;
using System.Collections.Generic;

namespace GalaxyCheck.Internal.Gens
{
    public abstract class BaseGen<T> : IGen<T>
    {
        private class GenAdvanced : IGenAdvanced<T>
        {
            private readonly BaseGen<T> _gen;

            public GenAdvanced(BaseGen<T> gen)
            {
                _gen = gen;
            }

            public IEnumerable<IGenIteration<T>> Run(GenParameters parameters) => _gen.Run(parameters);
        }

        public BaseGen()
        {
            Advanced = new GenAdvanced(this);
        }

        public IGenAdvanced<T> Advanced { get; }

        protected abstract IEnumerable<IGenIteration<T>> Run(GenParameters parameters);
    }
}
