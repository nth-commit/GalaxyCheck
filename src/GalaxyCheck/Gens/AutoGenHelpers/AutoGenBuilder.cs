﻿using GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories;
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
            Func<string, IGen> errorFactory,
            AutoGenHandlerContext context)
        {
            Func<string, AutoGenHandlerContext, IGen> contextualErrorFactory = (message, context) =>
            {
                var suffix = context.Members.Count() == 1 ? "" : $" at path '{context.MemberPath}'";
                return errorFactory(message + suffix);
            };

            var genFactoriesByPriority = new List<IAutoGenHandler>
            {
                new MemberOverrideAutoGenHandler(memberOverrides),
                new RegistryAutoGenHandler(registeredGensByType),
                new ListAutoGenHandler(),
                new DefaultConstructorAutoGenHandler(contextualErrorFactory),
                new NonDefaultConstructorAutoGenHandler(contextualErrorFactory),
            };

            var compositeAutoGenFactory = new CompositeAutoGenHandler(genFactoriesByPriority, contextualErrorFactory);

            return compositeAutoGenFactory.CreateGen(type, context);
        }
    }
}