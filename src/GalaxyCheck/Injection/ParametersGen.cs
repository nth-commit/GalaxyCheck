using GalaxyCheck.Gens;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using GalaxyCheck.Internal.Sizing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Injection
{
    /// <summary>
    /// TODO:
    /// 1) Validation of misplaced generation attributes
    /// 2) More injection types
    /// 3) Nested primitive configuration
    /// </summary>
    public class ParametersGen : BaseGen<object[]>, IGen<object[]>
    {
        private readonly Lazy<IGen<object[]>> _lazyGen;

        public ParametersGen(MethodInfo methodInfo)
        {
            _lazyGen = new Lazy<IGen<object[]>>(() => CreateGen(methodInfo));
        }

        protected override IEnumerable<IGenIteration<object[]>> Run(IRng rng, Size size) =>
            _lazyGen.Value.Advanced.Run(rng, size);

        private static IGen<object[]> CreateGen(MethodInfo methodInfo)
        {
            var gens = methodInfo
                .GetParameters()
                .Select(parameterInfo => ResolveGen(parameterInfo));

            return Gen.Zip(gens).Select(xs => xs.ToArray());
        }

        private static IGen<object> ResolveGen(ParameterInfo parameterInfo)
        {
            var gensByType = new Dictionary<Type, Func<ParameterInfo, IGen<object>>>
            {
                { typeof(int), pi => CreateInt32Gen(pi) }
            };

            return gensByType[parameterInfo.ParameterType](parameterInfo);
        }

        private static IGen<object> CreateInt32Gen(ParameterInfo parameterInfo)
        {
            var attributes = parameterInfo
                .GetCustomAttributes()
                .OfType<IGenInjectionConfigurationFilter<int, IInt32Gen>>();

            return attributes
                .Aggregate(
                    Gen.Int32(),
                    (gen, attr) => attr.Configure(gen))
                .Select(x => (object)x);
        }
    }
}
