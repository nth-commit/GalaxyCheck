using GalaxyCheck.Gens.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using System.Collections.Generic;

namespace GalaxyCheck.Gens.Internal
{
    internal abstract class BaseGen<T> : IGen<T>
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

    internal abstract record BaseGenV2<T> : IGen<T>
    {
        public IGenAdvanced<T> Advanced => new GenAdvanced(this);

        IGenAdvanced IGen.Advanced => Advanced;

        protected abstract IEnumerable<IGenIteration<T>> Run(GenParameters parameters);

        private class GenAdvanced : IGenAdvanced<T>
        {
            private readonly BaseGenV2<T> _gen;

            public GenAdvanced(BaseGenV2<T> gen)
            {
                _gen = gen;
            }

            public IEnumerable<IGenIteration<T>> Run(GenParameters parameters) => _gen.Run(parameters);

            IEnumerable<IGenIteration> IGenAdvanced.Run(GenParameters parameters) => _gen.Run(parameters);
        }
    }
}
