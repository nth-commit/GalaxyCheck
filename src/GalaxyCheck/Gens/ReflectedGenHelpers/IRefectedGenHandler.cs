using System;
using System.Collections.Immutable;

namespace GalaxyCheck.Gens.ReflectedGenHelpers
{
    internal interface IReflectedGenHandler
    {
        bool CanHandleGen(Type type, ReflectedGenHandlerContext context);

        IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context);
    }

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

    internal static class ReflectedGenHandlerExtensions
    {
        public static IGen CreateGen(
            this IReflectedGenHandler innerHandler,
            Type type,
            ReflectedGenHandlerContext context) =>
                innerHandler.CreateGen(innerHandler, type, context);
    }
}
