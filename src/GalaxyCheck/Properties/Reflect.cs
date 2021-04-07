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
            return Gen
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
                })
                .Select(test => new TestImpl<object[]>(
                    test.Func,
                    test.Input,
                    parameters => parameters));
        }

        private static IGen<Test<object[]>> ToBooleanProperty(MethodInfo methodInfo, object? target)
        {
            return Gen
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
                })
                .Select(test => new TestImpl<object[]>(
                    test.Func,
                    test.Input,
                    parameters => parameters));
        }

        private static IGen<Test<object[]>> ToNestedProperty(MethodInfo methodInfo, object? target) =>
            from parameters in Gen.Parameters(methodInfo)
            let property = InvokeNestedProperty(methodInfo, target, parameters)
            where property != null
            from test in property
            select new TestImpl<object[]>(
                _ => test.Func(test.Input),
                Enumerable.Concat(parameters, test.Input).ToArray(),
                _ => Enumerable.Concat(parameters, test.Input).ToArray());

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
