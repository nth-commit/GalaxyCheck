using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class MemberOverrideReflectedGenHandler : IReflectedGenHandler
    {
        private IReadOnlyList<ReflectedGenMemberOverride> _memberOverrides;
        private ImmutableHashSet<string> _memberPathOverrides;

        public MemberOverrideReflectedGenHandler(IReadOnlyList<ReflectedGenMemberOverride> memberOverrides)
        {
            _memberOverrides = memberOverrides;
            _memberPathOverrides = ImmutableHashSet.Create(_memberOverrides.Select(x => x.Path).ToArray());
        }

        public bool CanHandleGen(Type type, ReflectedGenHandlerContext context) =>
            _memberPathOverrides.Contains(context.MemberPath);

        public IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context) =>
            _memberOverrides
                .Where(mo => mo.Path == context.MemberPath)
                .Select(mo => mo.Gen)
                .Last(); // Last registered override wins
    }
}
