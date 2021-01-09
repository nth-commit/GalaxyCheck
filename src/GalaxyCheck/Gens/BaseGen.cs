using GalaxyCheck.Sizing;
using System.Collections.Generic;

namespace GalaxyCheck.Gens
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

            public IEnumerable<GenIteration<T>> Run(IRng rng, Size size) => _gen.Run(rng, size);
        }

        public BaseGen()
        {
            Advanced = new GenAdvanced(this);
        }

        public IGenAdvanced<T> Advanced { get; }

        protected abstract IEnumerable<GenIteration<T>> Run(IRng rng, Size size);
    }
}
