using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Gens.Parameters.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

        private static IEnumerable<IGen<Action<object>>> CreateSetPropertyActionGens(
            IReflectedGenHandler innerHandler,
            Type type,
            ReflectedGenHandlerContext context) => type
                .GetProperties()
                .Where(property => property.CanWrite)
                .Select(property => CreateSetMemberActionGen(
                    innerHandler,
                    context,
                    property.Name,
                    property.PropertyType,
                    (target, value) => property.SetValue(target, value)));

        private static IEnumerable<IGen<Action<object>>> CreateSetFieldActionGens(
            IReflectedGenHandler innerHandler,
            Type type,
            ReflectedGenHandlerContext context) => type
                .GetFields()
                .Where(field => field.IsPublic)
                .Select(field => CreateSetMemberActionGen(
                    innerHandler,
                    context,
                    field.Name,
                    field.FieldType,
                    (target, value) => field.SetValue(target, value)));

        private static IGen<Action<object>> CreateSetMemberActionGen(
            IReflectedGenHandler innerHandler,
            ReflectedGenHandlerContext parentContext,
            string memberName,
            Type memberType,
            Action<object, object> setMemberValue) => innerHandler.CreateNamedGen(memberType, memberName, parentContext)
                .Cast<object>()
                .Select((Func<object, Action<object>>)(value => obj => setMemberValue(obj, value)));
    }
}
