using GalaxyCheck.Gens;
using GalaxyCheck.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck
{
    public partial class Property
    {
        private delegate Property ReflectedPropertyHandler(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory,
            IReadOnlyDictionary<int, IGen>? customGens);

        public static ImmutableList<Type> SupportedReturnTypes => MethodPropertyHandlers.Keys.ToImmutableList();

        public static Property Reflect(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory = null,
            IReadOnlyDictionary<int, IGen>? customGens = null)
        {
            if (!SupportedReturnTypes.Contains(methodInfo.ReturnType))
            {
                var supportedReturnTypesFormatted = string.Join(", ", SupportedReturnTypes);
                var message = $"Return type {methodInfo.ReturnType} is not supported by GalaxyCheck. Please use one of: {supportedReturnTypesFormatted}";
                throw new Exception(message);
            }

            return MethodPropertyHandlers[methodInfo.ReturnType](methodInfo, target, genFactory, customGens);
        }

        private readonly static ImmutableDictionary<Type, ReflectedPropertyHandler> MethodPropertyHandlers =
            new Dictionary<Type, ReflectedPropertyHandler>
            {
                { typeof(void), ToVoidProperty },
                { typeof(bool), ToBooleanProperty },
                { typeof(Property), ToReturnedProperty },
                { typeof(IGen<Test>), ToPureProperty },
            }.ToImmutableDictionary();

        private static ReflectedPropertyHandler ToVoidProperty => (methodInfo, target, genFactory, customGens) =>
        {
            return new Property(Gen
                .Parameters(methodInfo, genFactory, customGens)
                .ForAll(parameters =>
                {
                    try
                    {
                        methodInfo.Invoke(target, parameters);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException!;
                    }
                })
                .Select(test => TestFactory.Create<object>(
                    test.Input,
                    test.Output,
                    test.Input)));
        };

        private static ReflectedPropertyHandler ToBooleanProperty => (methodInfo, target, genFactory, customGens) =>
        {
            return new Property(Gen
                .Parameters(methodInfo, genFactory, customGens)
                .ForAll(parameters =>
                {
                    try
                    {
                        return (bool)methodInfo.Invoke(target, parameters)!;
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException!;
                    }
                })
                .Select(test => TestFactory.Create<object>(
                    test.Input,
                    test.Output,
                    test.Input)));
        };

        private static ReflectedPropertyHandler ToReturnedProperty => (methodInfo, target, genFactory, customGens) =>
        {
            if (methodInfo.GetParameters().Any())
            {
                throw new Exception($"Parameters are not support for methods returning properties. Violating signature: \"{methodInfo}\"");
            }

            try
            {
                return (Property)methodInfo.Invoke(target, new object[] {})!;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException!;
            }
        };

        private static ReflectedPropertyHandler ToPureProperty => (methodInfo, target, _, _) =>
        {
            try
            {
                return new Property((IGen<Test>)methodInfo.Invoke(target, new object[] { })!);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException!;
            }
        };
    }
}
