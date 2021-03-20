namespace GalaxyCheck
{
    using GalaxyCheck.Gens;
    using System.Reflection;

    public static partial class Gen
    {
        /// <summary>
        /// Creates a generator that produces parameters to the given method. Can be used to dynamically invoke a
        /// method or a delegate.
        /// </summary>
        /// <param name="methodInfo">The method to generate parameters for.</param>
        /// <returns>The new generator.</returns>
        public static IGen<object[]> Parameters(MethodInfo methodInfo) => new ParametersGen(methodInfo);
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Injection;
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// TODO:
    /// 1) Validation of misplaced generation attributes
    /// 2) More injection types
    /// 3) Nested primitive configuration
    /// </summary>
    internal class ParametersGen : BaseGen<object[]>, IGen<object[]>
    {
        private readonly Lazy<IGen<object[]>> _lazyGen;

        public ParametersGen(MethodInfo methodInfo)
        {
            _lazyGen = new Lazy<IGen<object[]>>(() => CreateGen(methodInfo));
        }

        protected override IEnumerable<IGenIteration<object[]>> Run(GenParameters parameters) =>
            _lazyGen.Value.Advanced.Run(parameters);

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
