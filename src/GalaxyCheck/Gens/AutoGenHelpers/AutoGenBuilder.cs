using GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GalaxyCheck.Gens.AutoGenHelpers
{
    internal static class AutoGenBuilder
    {
        public static IGen Build(
            Type type,
            IReadOnlyDictionary<Type, IGen> registeredGensByType,
            IReadOnlyList<AutoGenMemberOverride> memberOverrides,
            Func<string, IGen> errorFactory,
            AutoGenFactoryContext context)
        {
            var genFactoriesByPriority = new List<IAutoGenFactory>
            {
                new MemberOverrideAutoGenFactory(memberOverrides),
                new RegistryAutoGenFactory(registeredGensByType),
                new ListAutoGenFactory(),
                new ConstructorParamsAutoGenFactory(),
                new PropertySettingAutoGenFactory()
            };

            var compositeAutoGenFactory = new CompositeAutoGenFactory(genFactoriesByPriority, errorFactory);

            return compositeAutoGenFactory.CreateGen(type, context);
        }
    }
}
