﻿using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Sizing;
using System;
using System.Collections.Generic;

namespace GalaxyCheck.Internal.Gens
{
    public delegate int NextIntFunc(int min, int max);

    public delegate T StatefulGenFunc<T>(NextIntFunc useNextInt, Size size);

    public class PrimitiveGen<T> : BaseGen<T>, IGen<T>
    {
        private readonly StatefulGenFunc<T> _generate;
        private readonly Func<T, IExampleSpace<T>> _unfold;

        public PrimitiveGen(
            StatefulGenFunc<T> generate,
            Func<T, IExampleSpace<T>> unfold)
        {
            _generate = generate;
            _unfold = unfold;
        }

        protected override IEnumerable<IGenIteration<T>> Run(GenParameters parameters)
        {
            NextIntFunc useNextInt = (min, max) =>
            {
                var value = parameters.Rng.Value(min, max);
                parameters = new GenParameters(parameters.Rng.Next(), parameters.Size);
                return value;
            };

            do
            {
                var initialParameters = parameters;

                var exampleSpace = _unfold(_generate(useNextInt, parameters.Size));

                yield return GenIterationFactory.Instance(initialParameters, parameters, exampleSpace);
            } while (true);
        }
    }
}