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
        /// <param name="autoGenFactory">The <see cref="IAutoGenFactory"/> to use to create generators from parameter
        /// types. If null, it uses the default factory.</param>
        /// <returns>The new generator.</returns>
        public static IGen<object[]> Parameters(MethodInfo methodInfo, IAutoGenFactory? autoGenFactory = null)
            => new ParametersGen(autoGenFactory ?? AutoFactory(), methodInfo);
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using GalaxyCheck.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

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

            return Gen.Zip(gens).Select(xs => xs.ToArray());
        }

        private static IGen<object> ResolveGen(IAutoGenFactory autoGenFactory, ParameterInfo parameterInfo)
        {
            var configuredAutoGenFactory = ConfigureAutoGenFactory(autoGenFactory, parameterInfo);

            var createAutoGenMethodInfo = typeof(IAutoGenFactory)
                .GetMethod(nameof(IAutoGenFactory.Create))
                .MakeGenericMethod(parameterInfo.ParameterType);

            var gen = (IGen)createAutoGenMethodInfo.Invoke(configuredAutoGenFactory, new object[] { });

            return gen
                .Cast<object>()
                .SelectError(error => SelectParameterError(parameterInfo, error));
        }

        private static IAutoGenFactory ConfigureAutoGenFactory(IAutoGenFactory autoGenFactory, ParameterInfo parameterInfo)
        {
            if (TryCreateGenFromAttribute(parameterInfo, out IGen? gen))
            {
                return autoGenFactory.RegisterType(parameterInfo.ParameterType, gen!);
            }

            return autoGenFactory;
        }

        private static bool TryCreateGenFromAttribute(ParameterInfo parameterInfo, out IGen? gen)
        {
            var genProviderAttributes = parameterInfo
                .GetCustomAttributes()
                .OfType<GenProviderAttribute>();

            var (genProviderAttribute, otherGenProviderAttributes) = genProviderAttributes;

            if (genProviderAttribute == null)
            {
                gen = null;
                return false;
            }

            if (otherGenProviderAttributes.Any())
            {
                gen = Gen.Advanced.Error(
                    parameterInfo.ParameterType,
                    nameof(ParametersGen),
                    $"multiple {nameof(GenProviderAttribute)}s is unsupported");
                return true;
            }

            gen = genProviderAttribute.Get;
            return true;
        }

        private static Iterations.IGenErrorData SelectParameterError(ParameterInfo parameterInfo, Iterations.IGenErrorData error)
        {
            return new GenErrorData(
                nameof(ParametersGen),
                $"unable to generate value for parameter '{parameterInfo.Name}', {error.Message}");
        }
    }
}
