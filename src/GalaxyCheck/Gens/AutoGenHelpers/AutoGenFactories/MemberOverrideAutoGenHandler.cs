using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories
{
    internal class MemberOverrideAutoGenHandler : IAutoGenHandler
    {
        private IReadOnlyList<AutoGenMemberOverride> _memberOverrides;
        private ImmutableHashSet<string> _memberPathOverrides;

        public MemberOverrideAutoGenHandler(IReadOnlyList<AutoGenMemberOverride> memberOverrides)
        {
            _memberOverrides = memberOverrides;
            _memberPathOverrides = ImmutableHashSet.Create(_memberOverrides.Select(x => x.Path).ToArray());
        }

        public bool CanHandleGen(Type type, AutoGenHandlerContext context) =>
            _memberPathOverrides.Contains(context.MemberPath);

        public IGen CreateGen(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context) =>
            _memberOverrides
                .Where(mo => mo.Path == context.MemberPath)
                .Select(mo => mo.Gen)
                .Last(); // Last registered override wins
    }
}
