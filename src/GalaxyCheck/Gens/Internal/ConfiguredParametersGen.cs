using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using System;
using System.Collections.Generic;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        internal static IGen<T> WithTransformedParameters<T>(
            this IGen<T> gen,
            Func<GenParameters, GenParameters> transformParameters) =>
                new ConfiguredParametersGen<T>(gen, transformParameters);

        internal static IGen WithTransformedParameters(
            this IGen gen,
            Func<GenParameters, GenParameters> transformParameters) =>
                new ConfiguredParametersGen(gen, transformParameters);
    }
}

namespace GalaxyCheck.Gens.Internal
{
    internal class ConfiguredParametersGen<T> : BaseGen<T>
    {
        private readonly IGen<T> _gen;
        private readonly Func<GenParameters, GenParameters> _transformParameters;

        public ConfiguredParametersGen(IGen<T> gen, Func<GenParameters, GenParameters> transformParameters)
        {
            _gen = gen;
            _transformParameters = transformParameters;
        }

        protected override IEnumerable<IGenIteration<T>> Run(GenParameters parameters) =>
            _gen.Advanced.Run(_transformParameters(parameters));
    }

    internal class ConfiguredParametersGen : BaseGen
    {
        private readonly IGen _gen;
        private readonly Func<GenParameters, GenParameters> _transformParameters;

        public ConfiguredParametersGen(IGen gen, Func<GenParameters, GenParameters> transformParameters)
        {
            _gen = gen;
            _transformParameters = transformParameters;
        }

        protected override IEnumerable<IGenIteration> Run(GenParameters parameters) =>
            _gen.Advanced.Run(_transformParameters(parameters));
    }
}
