using GalaxyCheck.Abstractions;
using System.Collections.Generic;

namespace GalaxyCheck.Gens
{
    public class BaseGenBuilder<T> : IGenBuilder<T>
    {
        private readonly IGen<T> _innerGen;

        public BaseGenBuilder(IGen<T> innerGen)
        {
            _innerGen = innerGen;
        }

        public IEnumerable<GenIteration<T>> Run(IRng rng)
        {
            return _innerGen.Run(rng);
        }
    }
}
