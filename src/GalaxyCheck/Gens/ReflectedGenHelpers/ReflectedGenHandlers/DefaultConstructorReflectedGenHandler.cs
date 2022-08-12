using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class DefaultConstructorReflectedGenHandler : IReflectedGenHandler
    {
        private readonly ContextualErrorFactory _errorFactory;

        public DefaultConstructorReflectedGenHandler(ContextualErrorFactory errorFactory)
        {
            _errorFactory = errorFactory;
        }

        public bool CanHandleGen(Type type, ReflectedGenHandlerContext context) =>
            type.GetConstructors().Any(constructor => constructor.GetParameters().Any() == false);

        public IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context)
        {
            var methodInfo = typeof(DefaultConstructorReflectedGenHandler).GetMethod(
                nameof(CreateGenGeneric),
                BindingFlags.Static | BindingFlags.NonPublic)!;

            var genericMethodInfo = methodInfo.MakeGenericMethod(type);

            return (IGen)genericMethodInfo.Invoke(null!, new object[] { innerHandler, context, _errorFactory })!;
        }

        private static IGen<T> CreateGenGeneric<T>(IReflectedGenHandler innerHandler, ReflectedGenHandlerContext context, ContextualErrorFactory errorFactory)
        {
            return Gen
                .Zip(
                    Gen.Zip(CreateSetPropertyActionGens(innerHandler, typeof(T), context)),
                    Gen.Zip(CreateSetFieldActionGens(innerHandler, typeof(T), context)))
                .SelectMany((x) =>
                {
                    T instance;
                    try
                    {
                        instance = (T)Activator.CreateInstance(typeof(T))!;
                    }
                    catch (TargetInvocationException ex)
                    {
                        var innerEx = ex.InnerException;
                        var message = $"'{innerEx!.GetType()}' was thrown while calling constructor with message '{innerEx.Message}'";
                        return errorFactory(message, context).Cast<T>();
                    }

                    foreach (var setPropertyAction in x.Item1)
                    {
                        try
                        {
                            setPropertyAction(instance);
                        }
                        catch (TargetInvocationException ex)
                        {
                            var innerEx = ex.InnerException;
                            var message = $"'{innerEx!.GetType()}' was thrown while setting property with message '{innerEx.Message}'";
                            return errorFactory(message, context).Cast<T>();
                        }
                    }

                    foreach (var setFieldAction in x.Item2)
                    {
                        setFieldAction(instance);
                    }

                    return Gen.Constant(instance);
                });
        }

        private static IEnumerable<IGen<Action<object>>> CreateSetPropertyActionGens(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext parentContext)
        {
            var nullabilityInfoContext = new NullabilityInfoContext();
            return type
                .GetProperties()
                .Where(property => property.CanWrite)
                .Select(property =>
                {
                    var context = parentContext.Next(property.Name, property.PropertyType, nullabilityInfoContext.Create(property));
                    return innerHandler
                        .CreateGen(property.PropertyType, context)
                        .Cast<object>()
                        .Advanced.ReferenceRngWaypoint(rngWaypoint => rngWaypoint.Influence(context.CalculateStableSeed()))
                        .Select((Func<object?, Action<object>>)(value => obj => property.SetValue(obj, value)));
                });
        }

        private static IEnumerable<IGen<Action<object>>> CreateSetFieldActionGens(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext parentContext)
        {
            var nullabilityInfoContext = new NullabilityInfoContext();
            return type
                .GetFields()
                .Where(field => field.IsPublic)
                .Select(field =>
                {
                    var context = parentContext.Next(field.Name, field.FieldType, nullabilityInfoContext.Create(field));
                    return innerHandler
                        .CreateGen(field.FieldType, context)
                        .Cast<object>()
                        .Advanced.ReferenceRngWaypoint(rngWaypoint => rngWaypoint.Influence(context.CalculateStableSeed()))
                        .Select((Func<object?, Action<object>>)(value => obj => field.SetValue(obj, value)));
                });
        }
    }
}
