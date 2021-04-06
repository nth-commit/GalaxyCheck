using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static ImmutableList<Type> SupportedReturnTypes => MethodPropertyHandlers.Keys.ToImmutableList();

        public static IGen<Test<object[]>> Reflect(MethodInfo methodInfo, object? target)
        {
            if (!SupportedReturnTypes.Contains(methodInfo.ReturnType))
            {
                var supportedReturnTypesFormatted = string.Join(", ", SupportedReturnTypes);
                var message = $"Return type {methodInfo.ReturnType} is not supported by GalaxyCheck.Xunit. Please use one of: {supportedReturnTypesFormatted}";
                throw new Exception(message);
            }

            return MethodPropertyHandlers[methodInfo.ReturnType](methodInfo, target);
        }

        private readonly static ImmutableDictionary<Type, Func<MethodInfo, object?, IGen<Test<object[]>>>> MethodPropertyHandlers =
            new Dictionary<Type, Func<MethodInfo, object?, IGen<Test<object[]>>>>
            {
                { typeof(void), ToVoidProperty },
                { typeof(bool), ToBooleanProperty },
                { typeof(Property), ToNestedProperty },
                { typeof(IGen<Test>), ToPureProperty }
            }.ToImmutableDictionary();

        private static IGen<Test<object[]>> ToVoidProperty(MethodInfo methodInfo, object? target)
        {
            var arity = methodInfo.GetParameters().Count();
            return GalaxyCheck.Gen
                .Parameters(methodInfo)
                .ForAll(parameters =>
                {
                    try
                    {
                        methodInfo.Invoke(target, parameters);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException;
                    }
                }, arity);
        }

        private static IGen<Test<object[]>> ToBooleanProperty(MethodInfo methodInfo, object? target)
        {
            var arity = methodInfo.GetParameters().Count();
            return GalaxyCheck.Gen
                .Parameters(methodInfo)
                .ForAll(parameters =>
                {
                    try
                    {
                        return (bool)methodInfo.Invoke(target, parameters);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException;
                    }
                }, arity);
        }

        private static IGen<Test<object[]>> ToNestedProperty(MethodInfo methodInfo, object? target) =>
            from parameters in GalaxyCheck.Gen.Parameters(methodInfo)
            let property = InvokeNestedProperty(methodInfo, target, parameters)
            where property != null
            from propertyIteration in property
            select new Property<object[]>.TestImpl(
                (x) => propertyIteration.Func(new object[] { x.Last() }),
                Enumerable.Concat(parameters, propertyIteration.Input).ToArray(),
                parameters.Count() + propertyIteration.Arity,
                false);

        private static Property? InvokeNestedProperty(MethodInfo methodInfo, object? target, object[] parameters)
        {
            try
            {
                return (Property)methodInfo.Invoke(target, parameters);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException.GetType() == typeof(Property.PropertyPreconditionException))
                {
                    return null;
                }

                throw ex.InnerException;
            }
        }

        private static IGen<Test<object[]>> ToPureProperty(MethodInfo methodInfo, object? target)
        {
            try
            {
                return (IGen<Test>)methodInfo.Invoke(target, new object[] { });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
