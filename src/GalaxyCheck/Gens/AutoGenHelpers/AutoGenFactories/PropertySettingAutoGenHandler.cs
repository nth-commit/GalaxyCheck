using System;
using System.Linq;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories
{
    internal class PropertySettingAutoGenHandler : IAutoGenHandler
    {
        public bool CanHandleGen(Type type, AutoGenHandlerContext context) =>
            type.BaseType == typeof(object);

        public IGen CreateGen(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context)
        {
            var setPropertyActionGens = type
                .GetProperties()
                .Select(property => innerHandler
                    .CreateGen(property.PropertyType, context.Next(property.Name, property.PropertyType))
                    .Cast<object>()
                    .Select<object, Action<object>>(value => obj => property.SetValue(obj, value)));

            return Gen
                .Zip(setPropertyActionGens)
                .Select(setPropertyActions =>
                {
                    var instance = Activator.CreateInstance(type);

                    foreach (var setPropertyAction in setPropertyActions)
                    {
                        setPropertyAction(instance);
                    }

                    return instance;
                });
        }
    }
}
