using GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Gens.ReflectedGenHelpers
{
    internal static class ReflectedGenBuilder
    {
        public static IGen Build(
            Type type,
            IReadOnlyDictionary<Type, Func<IGen>> registeredGensByType,
            IReadOnlyList<ReflectedGenMemberOverride> memberOverrides,
            ErrorFactory errorFactory,
            ReflectedGenHandlerContext context)
        {
            ContextualErrorFactory contextualErrorFactory = (message, context) =>
            {
                var suffix = context.Members.Count() == 1 ? "" : $" at path '{context.MemberPath}'";
                return errorFactory(message + suffix);
            };

            var genFactoriesByPriority = new List<IReflectedGenHandler>
            {
                new MemberOverrideReflectedGenHandler(memberOverrides),
                new NullableGenHandler(),
                new RegistryReflectedGenHandler(registeredGensByType, contextualErrorFactory),
                new CollectionReflectedGenHandler(),
                new ArrayReflectedGenHandler(),
                new EnumReflectedGenHandler(),
                new DefaultConstructorReflectedGenHandler(contextualErrorFactory),
                new NonDefaultConstructorReflectedGenHandler(contextualErrorFactory),
            };

            var compositeReflectedGenFactory = new CompositeReflectedGenHandler(genFactoriesByPriority, contextualErrorFactory);

            return compositeReflectedGenFactory.CreateGen(type, context);
        }
    }
}
