using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Sizing;
using System.Collections.Generic;

namespace GalaxyCheck.Internal.Gens
{
    public abstract class GenProvider<T> : BaseGen<T>
    {
        protected abstract IGen<T> Gen { get; }

        protected override IEnumerable<IGenIteration<T>> Run(IRng rng, Size size) => Gen.Advanced.Run(rng, size);
    }
}
