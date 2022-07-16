using GalaxyCheck.Gens;
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
        public static Property Reflect(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory = null,
            IReadOnlyDictionary<int, IGen>? customGens = null)
        {
            if (MethodPropertyHandlers.TryGetValue(methodInfo.ReturnType, out var handler) == false)
            {
                var supportedReturnTypesFormatted = FormatSupportReturnTypes(MethodPropertyHandlers.Keys);

                var message = $"Return type is not supported by {nameof(Property)}.{nameof(Reflect)}.";
                if (AsyncMethodPropertyHandlers.ContainsKey(methodInfo.ReturnType))
                {
                    message += $" Did you mean to use {nameof(Property)}.{nameof(ReflectAsync)}? Otherwise, please use one of: {supportedReturnTypesFormatted}.";
                }
                else { 
                    message += $" Please use one of: {supportedReturnTypesFormatted}.";
                }
                message += $" Return type was: {methodInfo.ReturnType}.";

                throw new Exception(message);
            }

            return handler(methodInfo, target, genFactory, customGens);
        }

        public static AsyncProperty ReflectAsync(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory = null,
            IReadOnlyDictionary<int, IGen>? customGens = null)
        {
            if (MethodPropertyHandlers.TryGetValue(methodInfo.ReturnType, out var handler) == false)
            {
                if (AsyncMethodPropertyHandlers.TryGetValue(methodInfo.ReturnType, out var asyncHandler) == false)
                {
                    var supportedReturnTypesFormatted = FormatSupportReturnTypes(Enumerable.Concat(MethodPropertyHandlers.Keys, AsyncMethodPropertyHandlers.Keys));
                    var message = $"Return type is not supported by {nameof(Property)}.{nameof(ReflectAsync)}. Please use one of: {supportedReturnTypesFormatted}. Return type was: {methodInfo.ReturnType}.";
                    throw new Exception(message);
                }

                return asyncHandler(methodInfo, target, genFactory, customGens);
            }
            else
            {
                var asyncHandler = ToAsyncPropertyHandler(handler);
                return asyncHandler(methodInfo, target, genFactory, customGens);
            }
        }

        private static string FormatSupportReturnTypes(IEnumerable<Type> types)
        {
            return string.Join(", ", types.OrderBy(x => x.FullName));
        }

        private delegate Property ReflectedPropertyHandler(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory,
            IReadOnlyDictionary<int, IGen>? customGens);

        private delegate AsyncProperty AsyncReflectedPropertyHandler(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory,
            IReadOnlyDictionary<int, IGen>? customGens);

        private static AsyncReflectedPropertyHandler ToAsyncPropertyHandler(ReflectedPropertyHandler handler)
        {
            return (methodInfo, target, genFactory, customGens) =>
            {
                var property = handler(methodInfo, target, genFactory, customGens);
                return new AsyncProperty(property.Select(test => test.AsAsync()));
            };
        }

        private readonly static ImmutableDictionary<Type, ReflectedPropertyHandler> MethodPropertyHandlers =
            new Dictionary<Type, ReflectedPropertyHandler>
            {
                { typeof(void), ToVoidProperty },
                { typeof(bool), ToBooleanProperty },
                { typeof(Property), ToReturnedProperty },
                { typeof(IGen<Test>), ToPureProperty },
            }.ToImmutableDictionary();

        private readonly static ImmutableDictionary<Type, AsyncReflectedPropertyHandler> AsyncMethodPropertyHandlers =
            new Dictionary<Type, AsyncReflectedPropertyHandler>
            {
                { typeof(Task), ToTaskProperty },
                { typeof(Task<bool>), ToTaskBooleanProperty },
                { typeof(AsyncProperty), ToReturnedAsyncProperty },
                { typeof(IGen<AsyncTest>), ToPureAsyncProperty },
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
                }));
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
                }));
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

        private static AsyncReflectedPropertyHandler ToTaskProperty => (methodInfo, target, genFactory, customGens) =>
        {
            return new AsyncProperty(Gen
                .Parameters(methodInfo, genFactory, customGens)
                .ForAllAsync(parameters =>
                {
                    try
                    {
                        return (Task)methodInfo.Invoke(target, parameters)!;
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException!;
                    }
                }));
        };

        private static AsyncReflectedPropertyHandler ToTaskBooleanProperty => (methodInfo, target, genFactory, customGens) =>
        {
            return new AsyncProperty(Gen
                .Parameters(methodInfo, genFactory, customGens)
                .ForAllAsync(parameters =>
                {
                    try
                    {
                        return (Task<bool>)methodInfo.Invoke(target, parameters)!;
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException!;
                    }
                }));
        };

        private static AsyncReflectedPropertyHandler ToReturnedAsyncProperty => (methodInfo, target, genFactory, customGens) =>
        {
            if (methodInfo.GetParameters().Any())
            {
                throw new Exception($"Parameters are not support for methods returning properties. Violating signature: \"{methodInfo}\"");
            }

            try
            {
                return (AsyncProperty)methodInfo.Invoke(target, new object[] { })!;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException!;
            }
        };

        private static AsyncReflectedPropertyHandler ToPureAsyncProperty => (methodInfo, target, _, _) =>
        {
            try
            {
                return new AsyncProperty((IGen<AsyncTest>)methodInfo.Invoke(target, new object[] { })!);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException!;
            }
        };
    }
}
