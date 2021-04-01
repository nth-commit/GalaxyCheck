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

            return Gen.Zip(gens.Select(g => g.Cast<object>())).Select(xs => xs.ToArray());
        }

        private static IGen ResolveGen(ParameterInfo parameterInfo)
        {
            var autoGenFactory = CreateAutoGenFactory(parameterInfo);

            var createAutoGenMethodInfo = typeof(IAutoGenFactory)
                .GetMethod(nameof(IAutoGenFactory.Create))
                .MakeGenericMethod(parameterInfo.ParameterType);

            return (IGen)createAutoGenMethodInfo.Invoke(autoGenFactory, new object[] { });
        }

        private static IAutoGenFactory CreateAutoGenFactory(ParameterInfo parameterInfo)
        {
            if (parameterInfo.ParameterType == typeof(int))
            {
                return Gen.AutoFactory().RegisterType(CreateInt32Gen(parameterInfo));
            }

            return Gen.AutoFactory();
        }

        private static IGen<int> CreateInt32Gen(ParameterInfo parameterInfo)
        {
            var attributes = parameterInfo
                .GetCustomAttributes()
                .OfType<IGenInjectionConfigurationFilter<int, IInt32Gen>>();

            return attributes
                .Aggregate(
                    Gen.Int32(),
                    (gen, attr) => attr.Configure(gen));
        }
    }
}
