using GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GalaxyCheck.Gens.AutoGenHelpers
{
    public static class AutoGenBuilder
    {
        public static IGen Build(
            Type type,
            ImmutableDictionary<Type, IGen> registeredGensByType,
            Func<string, IGen> errorFactory,
            ImmutableStack<(string name, Type type)> path)
        {
            var genFactoriesByPriority = new List<IAutoGenFactory>
            {
                new RegistryAutoGenFactory(registeredGensByType),
                new ListAutoGenFactory(),
                new ConstructorParamsAutoGenFactory(),
                new PropertySettingAutoGenFactory()
            };

            var compositeAutoGenFactory = new CompositeAutoGenFactory(genFactoriesByPriority, errorFactory);

            return compositeAutoGenFactory.CreateGen(type, path);
        }
    }
}
