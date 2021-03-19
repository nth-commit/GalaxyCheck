using GalaxyCheck.Internal.GenIterations;
using System.Collections.Generic;

namespace GalaxyCheck.Internal.Gens
{
    public abstract class GenProvider<T> : BaseGen<T>
    {
        protected abstract IGen<T> Gen { get; }

        protected override IEnumerable<IGenIteration<T>> Run(GenParameters parameters) => Gen.Advanced.Run(parameters);
    }
}
