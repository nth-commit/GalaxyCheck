using GalaxyCheck.Gens.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GalaxyCheck.Gens.Internal
{
    internal abstract record GenProvider<T> : IGen<T>
    {
        public IGenAdvanced<T> Advanced => new GenAdvanced(this);

        IGenAdvanced IGen.Advanced => Advanced;

        public abstract IGen<T> Get { get; }

        private class GenAdvanced : IGenAdvanced<T>
        {
            private readonly GenProvider<T> _gen;

            public GenAdvanced(GenProvider<T> gen)
            {
                _gen = gen;
            }

            public IEnumerable<IGenIteration<T>> Run(GenParameters parameters) => ProvidedGenCache.GetGenCached(_gen).Advanced.Run(parameters);

            IEnumerable<IGenIteration> IGenAdvanced.Run(GenParameters parameters) => ProvidedGenCache.GetGenCached(_gen).Advanced.Run(parameters);
        }
    }

    internal static class ProvidedGenCache
    {
        private static ConditionalWeakTable<IGen, IGen> _cache = new ConditionalWeakTable<IGen, IGen>();

        public static IGen<T> GetGenCached<T>(GenProvider<T> genProvider)
        {
            if (_cache.TryGetValue(genProvider, out var gen) == false)
            {
                gen = genProvider.Get;

                _cache.AddOrUpdate(genProvider, gen);
            }

            return (IGen<T>)gen;
        }
    }
}
