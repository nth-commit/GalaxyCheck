using GalaxyCheck.Gens.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using System;
using System.Collections.Generic;

namespace GalaxyCheck.Gens.Internal
{
    internal abstract record GenProvider<T> : IGen<T>
    {
        public IGenAdvanced<T> Advanced => new GenAdvanced(GetOrCreateLazy());
        IGenAdvanced IGen.Advanced => Advanced;

        protected abstract IGen<T> Get { get; }

        private Lazy<IGen<T>>? _lazyGen;
        private GenProvider<T>? _lazyGenInstance;
        private Lazy<IGen<T>> GetOrCreateLazy()
        {
            // Records don't call the base constructor on clone, so there's no hook to imperatively re-set the lazy if
            // it was invalidated by some properties of the implementing class updating. The solution to "lazily"
            // create the lazy, and store the ref that it was created against.

            if (_lazyGen == null || _lazyGenInstance != this)
            {
                _lazyGen = new Lazy<IGen<T>>(() => Get, isThreadSafe: true);
                _lazyGenInstance = this;
            }

            return _lazyGen;
        }

        private class GenAdvanced : IGenAdvanced<T>
        {
            private readonly Lazy<IGen<T>> _lazyGen;

            public GenAdvanced(Lazy<IGen<T>> lazyGen)
            {
                _lazyGen = lazyGen;
            }

            public IEnumerable<IGenIteration<T>> Run(GenParameters parameters) => _lazyGen.Value.Advanced.Run(parameters);

            IEnumerable<IGenIteration> IGenAdvanced.Run(GenParameters parameters) => _lazyGen.Value.Advanced.Run(parameters);
        }
    }

}
