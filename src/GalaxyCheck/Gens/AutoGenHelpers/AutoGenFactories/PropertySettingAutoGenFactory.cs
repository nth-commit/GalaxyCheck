using System;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories
{
    internal class PropertySettingAutoGenFactory : IAutoGenFactory
    {
        public bool CanHandleType(Type type) => type.BaseType == typeof(object);

        public IGen CreateGen(IAutoGenFactory innerFactory, Type type, ImmutableStack<(string name, Type type)> path)
        {
            var setPropertyActionGens = type
                .GetProperties()
                .Select(property => innerFactory
                    .CreateGen(property.PropertyType, path.Push((property.Name, property.PropertyType)))
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
