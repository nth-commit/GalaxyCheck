namespace GalaxyCheck
{
    using GalaxyCheck.Gens;

    public static partial class Gen
    {
        public static partial class Advanced
        {
            public static IGen<T> Error<T>(string genName, string message) => new ErrorGen<T>(genName, message);
        }
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Internal.GenIterations;
    using GalaxyCheck.Internal.Gens;
    using System.Collections.Generic;

    internal class ErrorGen<T> : BaseGen<T>, IGen<T>
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
                yield return GenIterationFactory.Error<T>(parameters, parameters, _genName, _message);
            }
        }
    }
}
