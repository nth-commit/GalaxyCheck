using GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories;
using System;
using System.Collections.Generic;

namespace GalaxyCheck.Gens.AutoGenHelpers
{
    internal static class AutoGenBuilder
    {
        public static IGen Build(
            Type type,
            IReadOnlyDictionary<Type, IGen> registeredGensByType,
            IReadOnlyList<AutoGenMemberOverride> memberOverrides,
            Func<string, IGen> errorFactory,
            AutoGenHandlerContext context)
        {
            var genFactoriesByPriority = new List<IAutoGenHandler>
            {
                new MemberOverrideAutoGenHandler(memberOverrides),
                new RegistryAutoGenHandler(registeredGensByType),
                new ListAutoGenHandler(),
                new DefaultConstructorAutoGenHandler(),
                new NonDefaultConstructorAutoGenHandler(),
            };

            var compositeAutoGenFactory = new CompositeAutoGenHandler(genFactoriesByPriority, errorFactory);

            return compositeAutoGenFactory.CreateGen(type, context);
        }
    }
}
