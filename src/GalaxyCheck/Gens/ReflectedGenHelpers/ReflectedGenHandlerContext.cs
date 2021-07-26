using System;
using System.Collections.Immutable;

namespace GalaxyCheck.Gens.ReflectedGenHelpers
{
    internal record ReflectedGenHandlerContext(
        ImmutableList<string> Members,
        ImmutableStack<Type> TypeHistory)
    {
        public static ReflectedGenHandlerContext Create(Type type) =>
            new ReflectedGenHandlerContext(ImmutableList.Create("$"), ImmutableStack.Create(type));

        public ReflectedGenHandlerContext Next(string memberName, Type type) =>
            new ReflectedGenHandlerContext(Members.Add(memberName), TypeHistory.Push(type));

        public string MemberPath => string.Join(".", Members);
    }
}
