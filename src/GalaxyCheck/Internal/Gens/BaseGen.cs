using GalaxyCheck.Internal.GenIterations;
using System.Collections.Generic;

namespace GalaxyCheck.Internal.Gens
{
    public abstract class BaseGen<T> : IGen<T>
    {
        public IGenAdvanced<T> Advanced => new GenAdvanced(this);

        IGenAdvanced IGen.Advanced => Advanced;

        protected abstract IEnumerable<IGenIteration<T>> Run(GenParameters parameters);

        private class GenAdvanced : IGenAdvanced<T>
        {
            private readonly BaseGen<T> _gen;

            public GenAdvanced(BaseGen<T> gen)
            {
                _gen = gen;
            }

            public IEnumerable<IGenIteration<T>> Run(GenParameters parameters) => _gen.Run(parameters);

            IEnumerable<IGenIteration> IGenAdvanced.Run(GenParameters parameters) => _gen.Run(parameters);
        }
    }
}
