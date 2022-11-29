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

        public bool CanHandleGen(Type type, ReflectedGenHandlerContext context)
        {
            if (IsStruct(type))
            {
                // Structs have slightly different constructor semantics.
                return type.GetConstructors().Any() == false;
            }
            else
            {
                return type.GetConstructors().Any(constructor => constructor.GetParameters().Any() == false);
            }
        }

        private static bool IsStruct(Type type) => type.IsValueType && !type.IsPrimitive && !type.IsEnum;

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
                    object instance;
                    try
                    {
                        instance = Activator.CreateInstance(typeof(T))!;
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
                            setPropertyAction(ref instance);
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
                        setFieldAction(ref instance);
                    }

                    return Gen.Constant((T)instance);
                });
        }
        
        private delegate void SetMemberAction(ref object instance);

        private static IEnumerable<IGen<SetMemberAction>> CreateSetPropertyActionGens(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext parentContext)
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
                        .Select((Func<object?, SetMemberAction>)(value => (ref object obj) => property.SetValue(obj, value)));
                });
        }

        private static IEnumerable<IGen<SetMemberAction>> CreateSetFieldActionGens(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext parentContext)
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
                        .Select((Func<object?, SetMemberAction>)(value => (ref object obj) => field.SetValue(obj, value)));
                });
        }
    }
}
