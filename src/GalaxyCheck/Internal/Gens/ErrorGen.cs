using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Sizing;
using System.Collections.Generic;

namespace GalaxyCheck.Internal.Gens
{
    public class ErrorGen<T> : BaseGen<T>, IGen<T>
    {
        private readonly string _genName;
        private readonly string _message;

        public ErrorGen(string genName, string message)
        {
            _genName = genName;
            _message = message;
        }

        protected override IEnumerable<IGenIteration<T>> Run(GenParameters parameters)
        {
            while (true)
            {
                yield return new GenError<T>(parameters, parameters, _genName, _message);
            }
        }
    }
}
