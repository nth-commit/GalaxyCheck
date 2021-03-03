﻿using GalaxyCheck.Internal.GenIterations;
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
                yield return GenIterationFactory.Error<T>(parameters, parameters, _genName, _message);
            }
        }
    }
}
