using System;
using System.Linq;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories
{
    internal class PropertySettingAutoGenFactory : IAutoGenFactory
    {
        public bool CanHandleType(Type type, AutoGenFactoryContext context) =>
            type.BaseType == typeof(object);

        public IGen CreateGen(IAutoGenFactory innerFactory, Type type, AutoGenFactoryContext context)
        {
            var setPropertyActionGens = type
                .GetProperties()
                .Select(property => innerFactory
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
