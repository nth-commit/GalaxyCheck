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
        public static IGen<object[]> Parameters(MethodInfo methodInfo, IAutoGenFactory? autoGenFactory = null)
            => new ParametersGen(autoGenFactory ?? AutoFactory(), methodInfo);
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

        public ParametersGen(IAutoGenFactory autoGenFactory, MethodInfo methodInfo)
        {
            _lazyGen = new Lazy<IGen<object[]>>(() => CreateGen(autoGenFactory, methodInfo));
        }

        protected override IEnumerable<IGenIteration<object[]>> Run(GenParameters parameters) =>
            _lazyGen.Value.Advanced.Run(parameters);

        private static IGen<object[]> CreateGen(IAutoGenFactory autoGenFactory, MethodInfo methodInfo)
        {
            var gens = methodInfo
                .GetParameters()
                .Select(parameterInfo => ResolveGen(autoGenFactory, parameterInfo));

            return Gen.Zip(gens.Select(g => g.Cast<object>())).Select(xs => xs.ToArray());
        }

        private static IGen ResolveGen(IAutoGenFactory autoGenFactory, ParameterInfo parameterInfo)
        {
            var configuredAutoGenFactory = ConfigureAutoGenFactory(autoGenFactory, parameterInfo);

            var createAutoGenMethodInfo = typeof(IAutoGenFactory)
                .GetMethod(nameof(IAutoGenFactory.Create))
                .MakeGenericMethod(parameterInfo.ParameterType);

            return (IGen)createAutoGenMethodInfo.Invoke(configuredAutoGenFactory, new object[] { });
        }

        private static IAutoGenFactory ConfigureAutoGenFactory(IAutoGenFactory autoGenFactory, ParameterInfo parameterInfo)
        {
            if (parameterInfo.ParameterType == typeof(int) && TryCreateInt32Gen(parameterInfo, out IGen<int>? gen))
            {
                return autoGenFactory.RegisterType(gen!);
            }

            return autoGenFactory;
        }

        private static bool TryCreateInt32Gen(ParameterInfo parameterInfo, out IGen<int>? gen)
        {
            var attributes = parameterInfo
                .GetCustomAttributes()
                .OfType<IGenInjectionConfigurationFilter<int, IInt32Gen>>();

            if (attributes.Any())
            {
                gen = attributes.Aggregate(Gen.Int32(), (gen, attr) => attr.Configure(gen));
                return true;
            }

            gen = null;
            return false;
        }
    }
}
