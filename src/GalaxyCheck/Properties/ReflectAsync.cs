using GalaxyCheck.Gens;
using GalaxyCheck.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GalaxyCheck
{
    public partial class Property
    {
        private delegate IGen<TestAsync<object>> ReflectedPropertyHandlerAsync(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory,
            IReadOnlyDictionary<int, IGen>? customGens);

        public static ImmutableList<Type> SupportedReturnTypesAsync => MethodPropertyHandlersAsync.Keys.ToImmutableList();

        public static IGen<TestAsync<object>> ReflectAsync(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory = null,
            IReadOnlyDictionary<int, IGen>? customGens = null)
        {
            if (!SupportedReturnTypesAsync.Contains(methodInfo.ReturnType))
            {
                var supportedReturnTypesFormatted = string.Join(", ", SupportedReturnTypesAsync);
                var message = $"Return type {methodInfo.ReturnType} is not supported by GalaxyCheck. Please use one of: {supportedReturnTypesFormatted}";
                throw new Exception(message);
            }

            return MethodPropertyHandlersAsync[methodInfo.ReturnType](methodInfo, target, genFactory, customGens);
        }

        private readonly static ImmutableDictionary<Type, ReflectedPropertyHandlerAsync> MethodPropertyHandlersAsync =
            new Dictionary<Type, ReflectedPropertyHandlerAsync>
            {
                { typeof(Task), ToVoidPropertyAsync },
                { typeof(Task<bool>), ToBooleanPropertyAsync },
                { typeof(Task<Property>), ToNestedPropertyAsync }, // TODO
                { typeof(IGen<TestAsync>), ToPurePropertyAsync }
            }.ToImmutableDictionary();

        private static ReflectedPropertyHandlerAsync ToVoidPropertyAsync => (methodInfo, target, genFactory, customGens) =>
        {
            return Gen
                .Parameters(methodInfo, genFactory, customGens)
                .ForAllAsync(async parameters =>
                {
                    try
                    {
                        await (Task)methodInfo.Invoke(target, parameters)!;
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException!;
                    }
                })
                .Select(test => TestFactory.CreateAsync<object>(
                    test.Input,
                    test.Output,
                    test.Input));
        };

        private static ReflectedPropertyHandlerAsync ToBooleanPropertyAsync => (methodInfo, target, genFactory, customGens) =>
        {
            return Gen
                .Parameters(methodInfo, genFactory, customGens)
                .ForAllAsync(async parameters =>
                {
                    try
                    {
                        return await (Task<bool>)methodInfo.Invoke(target, parameters)!;
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException!;
                    }
                })
                .Select(test => TestFactory.CreateAsync<object>(
                    test.Input,
                    test.Output,
                    test.Input));
        };

        private static ReflectedPropertyHandlerAsync ToNestedPropertyAsync => (methodInfo, target, genFactory, customGens) =>
            from parameters in Gen.Parameters(methodInfo, genFactory, customGens)
            let property = InvokeNestedPropertyAsync(methodInfo, target, parameters)
            where property != null
            from test in property
            select TestFactory.CreateAsync(
                test.Input,
                test.Output,
                Enumerable.Concat(parameters, test.PresentedInput!).ToArray());

        private static ReflectedPropertyHandlerAsync ToPurePropertyAsync => (methodInfo, target, _, _) =>
        {
            try
            {
                return ((IGen<TestAsync>)methodInfo.Invoke(target, new object[] { })!).Select(test => test.Cast<object>());
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException!;
            }
        };

        private static PropertyAsync? InvokeNestedPropertyAsync(MethodInfo methodInfo, object? target, object[] parameters)
        {
            try
            {
                return (PropertyAsync)methodInfo.Invoke(target, parameters)!;
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException!.GetType() == typeof(PropertyPreconditionException))
                {
                    return null;
                }

                throw ex.InnerException;
            }
        }
    }
}
