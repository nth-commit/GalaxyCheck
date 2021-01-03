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

        public IEnumerable<GenIteration<T>> Run(IRng rng)
        {
            while (true)
            {
                yield return new GenError<T>(_genName, _message);
            }
        }
    }
}
