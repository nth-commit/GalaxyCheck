﻿using GalaxyCheck.Gens;
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
        private delegate IGen<Test<object>> ReflectedPropertyHandler(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory);

        public static ImmutableList<Type> SupportedReturnTypes => MethodPropertyHandlers.Keys.ToImmutableList();

        public static IGen<Test<object>> Reflect(MethodInfo methodInfo, object? target, IGenFactory? genFactory = null)
        {
            if (!SupportedReturnTypes.Contains(methodInfo.ReturnType))
            {
                var supportedReturnTypesFormatted = string.Join(", ", SupportedReturnTypes);
                var message = $"Return type {methodInfo.ReturnType} is not supported by GalaxyCheck.Xunit. Please use one of: {supportedReturnTypesFormatted}";
                throw new Exception(message);
            }

            return MethodPropertyHandlers[methodInfo.ReturnType](methodInfo, target, genFactory);
        }

        private readonly static ImmutableDictionary<Type, ReflectedPropertyHandler> MethodPropertyHandlers =
            new Dictionary<Type, ReflectedPropertyHandler>
            {
                { typeof(void), ToVoidProperty },
                { typeof(bool), ToBooleanProperty },
                { typeof(Property), ToNestedProperty },
                { typeof(IGen<Test>), ToPureProperty }
            }.ToImmutableDictionary();

        private static ReflectedPropertyHandler ToVoidProperty => (methodInfo, target, genFactory) =>
        {
            return Gen
                .Parameters(methodInfo, genFactory)
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
                .Select(test => TestFactory.Create<object>(
                    test.Input,
                    test.Output,
                    test.Input));
        };

        private static ReflectedPropertyHandler ToBooleanProperty => (methodInfo, target, genFactory) =>
        {
            return Gen
                .Parameters(methodInfo, genFactory)
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
                .Select(test => TestFactory.Create<object>(
                    test.Input,
                    test.Output,
                    test.Input));
        };

        private static ReflectedPropertyHandler ToNestedProperty => (methodInfo, target, genFactory) =>
            from parameters in Gen.Parameters(methodInfo, genFactory)
            let property = InvokeNestedProperty(methodInfo, target, parameters)
            where property != null
            from test in property
            select TestFactory.Create(
                test.Input,
                test.Output,
                Enumerable.Concat(parameters, test.PresentedInput).ToArray());

        private static ReflectedPropertyHandler ToPureProperty => (methodInfo, target, _) =>
        {
            try
            {
                return ((IGen<Test>)methodInfo.Invoke(target, new object[] { })).Select(test => test.Cast<object>());
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        };

        private static Property? InvokeNestedProperty(MethodInfo methodInfo, object? target, object[] parameters)
        {
            try
            {
                return (Property)methodInfo.Invoke(target, parameters);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException.GetType() == typeof(PropertyPreconditionException))
                {
                    return null;
                }

                throw ex.InnerException;
            }
        }
    }
}
