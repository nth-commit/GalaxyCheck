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
            ReflectedGenHandlerContext context)
        {
            var genFactoriesByPriority = new List<IReflectedGenHandler>
            {
                new MemberOverrideReflectedGenHandler(memberOverrides),
                new NullableGenHandler(),
                new RegistryReflectedGenHandler(registeredGensByType),
                new CollectionReflectedGenHandler(),
                new ArrayReflectedGenHandler(),
                new EnumReflectedGenHandler(),
                new DefaultConstructorReflectedGenHandler(),
                new NonDefaultConstructorReflectedGenHandler(),
            };

            var compositeReflectedGenFactory = new CompositeReflectedGenHandler(genFactoriesByPriority);

            try
            {
                return compositeReflectedGenFactory.CreateGen(type, context);
            }
            catch (Exception ex)
            {
                return Gen.Advanced.Error(type, $"{ex.Message} \n {ex.StackTrace}");
            }
        }
    }
}
