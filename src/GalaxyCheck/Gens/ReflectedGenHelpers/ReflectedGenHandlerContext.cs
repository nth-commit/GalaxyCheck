using System;
using System.Collections.Immutable;
using System.Reflection;

namespace GalaxyCheck.Gens.ReflectedGenHelpers
{
    internal record ReflectedGenHandlerContext(
        ImmutableList<string> Members,
        ImmutableStack<Type> TypeHistory,
        NullabilityInfo? NullabilityInfo)
    {
        public static ReflectedGenHandlerContext Create(Type type, NullabilityInfo? nullabilityInfo) =>
            new ReflectedGenHandlerContext(ImmutableList.Create("$"), ImmutableStack.Create(type), nullabilityInfo);

        public ReflectedGenHandlerContext Next(string memberName, Type type, NullabilityInfo nullabilityInfo) =>
            new ReflectedGenHandlerContext(Members.Add(memberName), TypeHistory.Push(type), nullabilityInfo);

        public string MemberPath => string.Join(".", Members);
    }
}
