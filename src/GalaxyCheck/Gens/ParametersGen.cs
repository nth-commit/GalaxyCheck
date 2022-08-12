namespace GalaxyCheck
{
    using GalaxyCheck.Gens;
    using System.Collections.Generic;
    using System.Reflection;

    public static partial class Gen
    {
        private static readonly IReadOnlyDictionary<int, IGen> DefaultGensByParameterIndex = new Dictionary<int, IGen>();

        /// <summary>
        /// Creates a generator that produces parameters to the given method. Can be used to dynamically invoke a
        /// method or a delegate.
        /// </summary>
        /// <param name="methodInfo">The method to generate parameters for.</param>
        /// <param name="genFactory">The <see cref="IGenFactory"/> to use to create generators from parameter
        /// types. If null, it uses the default factory.</param>
        /// <param name="customGens">The generator to use, keyed by the index of the parameter. Generators
        /// supplied through this collection will not be associated with <paramref name="genFactory"/>.</param>
        /// <returns>The new generator.</returns>
        public static IGen<object[]> Parameters(
            MethodInfo methodInfo,
            IGenFactory? genFactory = null,
            IReadOnlyDictionary<int, IGen>? customGens = null) =>
                new ParametersGen(methodInfo, genFactory ?? Factory(), customGens ?? DefaultGensByParameterIndex);
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

    internal record ParametersGen : GenProvider<object[]>, IGen<object[]>
    {
        private readonly MethodInfo _methodInfo;
        private readonly IGenFactory _genFactory;
        private readonly IReadOnlyDictionary<int, IGen> _customGens;

        public ParametersGen(MethodInfo methodInfo, IGenFactory genFactory, IReadOnlyDictionary<int, IGen> customGens)
        {
            _methodInfo = methodInfo;
            _genFactory = genFactory;
            _customGens = customGens;
        }

        protected override IGen<object[]> Get => CreateGen(_methodInfo, _genFactory, _customGens);

        private static IGen<object[]> CreateGen(
            MethodInfo methodInfo,
            IGenFactory genFactory,
            IReadOnlyDictionary<int, IGen> customGens)
        {
            var parameters = methodInfo.GetParameters();

            var invalidIndicies = customGens.Keys.Except(Enumerable.Range(0, parameters.Length));
            if (invalidIndicies.Any())
            {
                return Gen.Advanced.Error<object[]>(
                    nameof(ParametersGen),
                    $"parameter index '{invalidIndicies.First()}' of custom generator was not valid");
            }

            var gens = parameters.Select((parameterInfo, parameterIndex) =>
            {
                var customGen = customGens.ContainsKey(parameterIndex) ? customGens[parameterIndex] : null;
                return ResolveGen(genFactory, customGen, parameterInfo);
            });

            return Gen.Zip(gens).Select(xs => xs.ToArray());
        }

        private static IGen<object> ResolveGen(IGenFactory genFactory, IGen? customGen, ParameterInfo parameterInfo)
        {
            if (customGen == null)
            {
                var configuredGenFactory = ConfigureGenFactory(genFactory, parameterInfo);

                var createGenMethodInfo = typeof(IGenFactory)
                    .GetMethod(nameof(IGenFactory.Create))!
                    .MakeGenericMethod(parameterInfo.ParameterType);

                var nullabilityInfoContext = new NullabilityInfoContext();
                var nullabilityInfo = nullabilityInfoContext.Create(parameterInfo);

                var gen = (IGen)createGenMethodInfo.Invoke(configuredGenFactory, new object[] { nullabilityInfo })!;

                return gen
                    .Cast<object>()
                    .SelectError(error => SelectParameterError(parameterInfo, error));
            }
            else
            {
                var genTypeArgument = customGen.ReflectGenTypeArgument();

                if (parameterInfo.ParameterType.IsAssignableFrom(genTypeArgument) == false)
                {
                    return Gen.Advanced.Error<object>(
                        nameof(ParametersGen),
                        $"generator of type '{genTypeArgument.FullName}' is not compatible with parameter of type '{parameterInfo.ParameterType.FullName}'");
                }

                return customGen.Cast<object>();
            }
        }

        private static IGenFactory ConfigureGenFactory(IGenFactory genFactory, ParameterInfo parameterInfo)
        {
            if (TryCreateGenFromAttribute(parameterInfo, out IGen? gen))
            {
                return genFactory.RegisterType(parameterInfo.ParameterType, gen!);
            }

            return genFactory;
        }

        private static bool TryCreateGenFromAttribute(ParameterInfo parameterInfo, out IGen? gen)
        {
            var genAttributes = parameterInfo
                .GetCustomAttributes()
                .OfType<GenAttribute>();

            var (genAttribute, otherGenAttributes) = genAttributes;

            if (genAttribute == null)
            {
                gen = null;
                return false;
            }

            if (otherGenAttributes.Any())
            {
                gen = Gen.Advanced.Error(
                    parameterInfo.ParameterType,
                    nameof(ParametersGen),
                    $"multiple {nameof(GenAttribute)}s is unsupported");
                return true;
            }

            gen = genAttribute.Value;
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
