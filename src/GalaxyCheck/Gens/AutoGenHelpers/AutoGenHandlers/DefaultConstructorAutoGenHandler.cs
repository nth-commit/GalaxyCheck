using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenHandlers
{
    internal class DefaultConstructorAutoGenHandler : IAutoGenHandler
    {
        private readonly ContextualErrorFactory _errorFactory;

        public DefaultConstructorAutoGenHandler(ContextualErrorFactory errorFactory)
        {
            _errorFactory = errorFactory;
        }

        public bool CanHandleGen(Type type, AutoGenHandlerContext context) =>
            type.GetConstructors().Any(constructor => constructor.GetParameters().Any() == false);

        public IGen CreateGen(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context)
        {
            var methodInfo = typeof(DefaultConstructorAutoGenHandler).GetMethod(
                nameof(CreateGenGeneric),
                BindingFlags.Static | BindingFlags.NonPublic)!;

            var genericMethodInfo = methodInfo.MakeGenericMethod(type);

            return (IGen)genericMethodInfo.Invoke(null!, new object[] { innerHandler, context, _errorFactory });
        }

        private static IGen<T> CreateGenGeneric<T>(IAutoGenHandler innerHandler, AutoGenHandlerContext context, ContextualErrorFactory errorFactory)
        {
            T instance;
            try
            {
                instance = (T)Activator.CreateInstance(typeof(T));
            }
            catch (TargetInvocationException ex)
            {
                var innerEx = ex.InnerException;
                var message = $"'{innerEx.GetType()}' was thrown while calling constructor with message '{innerEx.Message}'";
                return errorFactory(message, context).Cast<T>();
            }

            return Gen
                .Zip(
                    Gen.Zip(CreateSetPropertyActionGens(innerHandler, typeof(T), context)),
                    Gen.Zip(CreateSetFieldActionGens(innerHandler, typeof(T), context)))
                .SelectMany((x) =>
                {
                    foreach (var setPropertyAction in x.Item1)
                    {
                        try
                        {
                            setPropertyAction(instance);
                        }
                        catch (TargetInvocationException ex)
                        {
                            var innerEx = ex.InnerException;
                            var message = $"'{innerEx.GetType()}' was thrown while setting property with message '{innerEx.Message}'";
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

        private static IEnumerable<IGen<Action<object>>> CreateSetPropertyActionGens(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context)
        {
            return type
                .GetProperties()
                .Where(property => property.CanWrite)
                .Select(property => innerHandler
                    .CreateGen(property.PropertyType, context.Next(property.Name, property.PropertyType))
                    .Cast<object>()
                    .Select<object, Action<object>>(value => obj => property.SetValue(obj, value)));
        }

        private static IEnumerable<IGen<Action<object>>> CreateSetFieldActionGens(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context)
        {
            return type
                .GetFields()
                .Where(field => field.IsPublic)
                .Select(field => innerHandler
                    .CreateGen(field.FieldType, context.Next(field.Name, field.FieldType))
                    .Cast<object>()
                    .Select<object, Action<object>>(value => obj => field.SetValue(obj, value)));
        }
    }
}
