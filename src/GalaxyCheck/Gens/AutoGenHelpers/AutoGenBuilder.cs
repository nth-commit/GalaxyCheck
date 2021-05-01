using GalaxyCheck.Gens.AutoGenHelpers.AutoGenHandlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Gens.AutoGenHelpers
{
    internal static class AutoGenBuilder
    {
        public static IGen Build(
            Type type,
            IReadOnlyDictionary<Type, IGen> registeredGensByType,
            IReadOnlyList<AutoGenMemberOverride> memberOverrides,
            ErrorFactory errorFactory,
            AutoGenHandlerContext context)
        {
            ContextualErrorFactory contextualErrorFactory = (message, error, context) =>
            {
                var suffix = context.Members.Count() == 1 ? "" : $" at path '{context.MemberPath}'";
                return errorFactory(message + suffix, error);
            };

            var genFactoriesByPriority = new List<IAutoGenHandler>
            {
                new MemberOverrideAutoGenHandler(memberOverrides),
                new RegistryAutoGenHandler(registeredGensByType, contextualErrorFactory),
                new ListAutoGenHandler(),
                new ArrayAutoGenHandler(),
                new EnumAutoGenHandler(),
                new DefaultConstructorAutoGenHandler(contextualErrorFactory),
                new NonDefaultConstructorAutoGenHandler(contextualErrorFactory),
            };

            var compositeAutoGenFactory = new CompositeAutoGenHandler(genFactoriesByPriority, contextualErrorFactory);

            return compositeAutoGenFactory.CreateGen(type, context);
        }
    }
}
