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

        public static Property Reflect(MethodInfo methodInfo, object? target)
        {
            if (!SupportedReturnTypes.Contains(methodInfo.ReturnType))
            {
                var supportedReturnTypesFormatted = string.Join(", ", SupportedReturnTypes);
                var message = $"Return type {methodInfo.ReturnType} is not supported by GalaxyCheck.Xunit. Please use one of: {supportedReturnTypesFormatted}";
                throw new Exception(message);
            }

            return MethodPropertyHandlers[methodInfo.ReturnType](methodInfo, target);
        }

        private readonly static ImmutableDictionary<Type, Func<MethodInfo, object?, Property>> MethodPropertyHandlers =
            new Dictionary<Type, Func<MethodInfo, object?, Property>>
            {
                { typeof(void), ToVoidProperty },
                { typeof(bool), ToBooleanProperty },
                { typeof(Property), ToNestedProperty },
                { typeof(IGen<Test>), ToPureProperty }
            }.ToImmutableDictionary();

        private static Property ToVoidProperty(MethodInfo methodInfo, object? target)
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

        private static Property ToBooleanProperty(MethodInfo methodInfo, object? target)
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

        private static Property ToNestedProperty(MethodInfo methodInfo, object? target)
        {
            var gen =
                from parameters in GalaxyCheck.Gen.Parameters(methodInfo)
                let property = InvokeNestedProperty(methodInfo, target, parameters)
                where property != null
                from propertyIteration in property
                select new Property<object[]>.TestImpl(
                    (x) => propertyIteration.Func(x.Last()),
                    parameters.Append(propertyIteration.Input).ToArray(),
                    parameters.Count() + propertyIteration.Arity,
                    false);

            return new Property<object[]>(gen);
        }

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

        private static Property ToPureProperty(MethodInfo methodInfo, object? target)
        {
            try
            {
                return new Property((IGen<Test>)methodInfo.Invoke(target, new object[] { }));
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
