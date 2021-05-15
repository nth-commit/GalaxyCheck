using GalaxyCheck.Gens.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using System.Collections.Generic;

namespace GalaxyCheck.Gens.Internal
{
    internal abstract record GenProvider<T> : IGen<T>
    {
        public IGenAdvanced<T> Advanced => new GenAdvanced(this);

        IGenAdvanced IGen.Advanced => Advanced;

        protected abstract IGen<T> Get { get; }

        private class GenAdvanced : IGenAdvanced<T>
        {
            private readonly GenProvider<T> _gen;

            public GenAdvanced(GenProvider<T> gen)
            {
                _gen = gen;
            }

            public IEnumerable<IGenIteration<T>> Run(GenParameters parameters) => _gen.Get.Advanced.Run(parameters);

            IEnumerable<IGenIteration> IGenAdvanced.Run(GenParameters parameters) => _gen.Get.Advanced.Run(parameters);
        }
    }
}
