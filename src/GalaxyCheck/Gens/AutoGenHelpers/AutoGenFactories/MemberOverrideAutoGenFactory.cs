using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories
{
    internal class MemberOverrideAutoGenFactory : IAutoGenFactory
    {
        private IReadOnlyList<AutoGenMemberOverride> _memberOverrides;
        private ImmutableHashSet<string> _memberPathOverrides;

        public MemberOverrideAutoGenFactory(IReadOnlyList<AutoGenMemberOverride> memberOverrides)
        {
            _memberOverrides = memberOverrides;
            _memberPathOverrides = ImmutableHashSet.Create(_memberOverrides.Select(x => x.Path).ToArray());
        }

        public bool CanHandleType(Type type, AutoGenFactoryContext context) =>
            _memberPathOverrides.Contains(context.MemberPath);

        public IGen CreateGen(IAutoGenFactory innerFactory, Type type, AutoGenFactoryContext context) =>
            _memberOverrides
                .Where(mo => mo.Path == context.MemberPath)
                .Select(mo => mo.Gen)
                .Last(); // Last registered override wins
    }
}
