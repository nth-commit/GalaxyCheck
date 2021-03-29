using System;
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
            var setPropertyActionGens = type
                .GetProperties()
                .Where(property => property.CanWrite)
                .Select(property => innerHandler
                    .CreateGen(property.PropertyType, context.Next(property.Name, property.PropertyType))
                    .Cast<object>()
                    .Select<object, Action<object>>(value => obj => property.SetValue(obj, value)));

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

            return Gen.Zip(setPropertyActionGens).SelectMany(setPropertyActions =>
            {
                foreach (var setPropertyAction in setPropertyActions)
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

                return Gen.Constant(instance);
            });
        }
    }
}
