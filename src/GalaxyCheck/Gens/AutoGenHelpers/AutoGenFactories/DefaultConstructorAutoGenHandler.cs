using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories
{
    internal class DefaultConstructorAutoGenHandler : IAutoGenHandler
    {
        private readonly Func<string, AutoGenHandlerContext, IGen> _errorFactory;

        public DefaultConstructorAutoGenHandler(Func<string, AutoGenHandlerContext, IGen> errorFactory)
        {
            _errorFactory = errorFactory;
        }

        public bool CanHandleGen(Type type, AutoGenHandlerContext context) =>
            type.GetConstructors().Any(constructor => constructor.GetParameters().Any() == false);

        public IGen CreateGen(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context)
        {
            object instance;
            try
            {
                instance = Activator.CreateInstance(type);
            }
            catch (TargetInvocationException ex)
            {
                var innerEx = ex.InnerException;
                var message = $"'{innerEx.GetType()}' was thrown while calling constructor with message '{innerEx.Message}'";
                return _errorFactory(message, context);
            }

            return Gen
                .Zip(
                    Gen.Zip(CreateSetPropertyActionGens(innerHandler, type, context)),
                    Gen.Zip(CreateSetFieldActionGens(innerHandler, type, context)))
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
                            return _errorFactory(message, context).Cast<object>();
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
