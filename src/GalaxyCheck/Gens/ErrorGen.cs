using GalaxyCheck.Abstractions;
using System.Collections.Generic;

namespace GalaxyCheck.Gens
{
    public class ErrorGen<T> : IGen<T>
    {
        private readonly string _genName;
        private readonly string _message;

        public ErrorGen(string genName, string message)
        {
            _genName = genName;
            _message = message;
        }

        public IEnumerable<GenIteration<T>> Run(IRng rng, ISize size)
        {
            while (true)
            {
                yield return new GenError<T>(rng, rng, _genName, _message);
            }
        }
    }
}
