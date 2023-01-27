using System;
using GalaxyCheck.Gens.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using System.Collections.Generic;
using GalaxyCheck.Gens.Internal.Iterations;

namespace GalaxyCheck.Gens.Internal
{
    internal abstract record BaseGen<T> : IGen<T>
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

            public IEnumerable<IGenIteration<T>> Run(GenParameters parameters)
            {
                using var enumerator = _gen.Run(parameters).GetEnumerator();

                bool hasNext = true;
                IGenIteration<T>? prev = null;

                while (hasNext)
                {
                    Exception? ex = null;
                    try
                    {
                        hasNext = enumerator.MoveNext();
                    }
                    catch (Exception e)
                    {
                        ex = e;
                        hasNext = true;
                    }

                    if (ex is not null)
                    {
                        var replayParameters = prev?.ReplayParameters ?? parameters;

                        var message = $@"Exception thrown.

{ex}";
                        
                        yield return GenIterationFactory.Error<T>(replayParameters, message);
                        yield break;
                    }

                    if (hasNext)
                    {
                        prev = enumerator.Current;
                        yield return prev;
                    }
                }
            }

            IEnumerable<IGenIteration> IGenAdvanced.Run(GenParameters parameters) => Run(parameters);
        }
    }

    internal abstract record BaseGen : IGen
    {
        IGenAdvanced IGen.Advanced => new GenAdvanced(this);

        protected abstract IEnumerable<IGenIteration> Run(GenParameters parameters);

        private class GenAdvanced : IGenAdvanced
        {
            private readonly BaseGen _gen;

            public GenAdvanced(BaseGen gen)
            {
                _gen = gen;
            }

            IEnumerable<IGenIteration> IGenAdvanced.Run(GenParameters parameters) => _gen.Run(parameters);
        }
    }
}
