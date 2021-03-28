using System;
using System.Collections.Immutable;

namespace GalaxyCheck.Gens.AutoGenHelpers
{
    internal interface IAutoGenHandler
    {
        bool CanHandleGen(Type type, AutoGenHandlerContext context);

        IGen CreateGen(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context);
    }

    internal record AutoGenHandlerContext(
        ImmutableList<string> Members,
        ImmutableStack<Type> TypeHistory)
    {
        public static AutoGenHandlerContext Create(Type type) =>
            new AutoGenHandlerContext(ImmutableList.Create("$"), ImmutableStack.Create(type));

        public AutoGenHandlerContext Next(string memberName, Type type) =>
            new AutoGenHandlerContext(Members.Add(memberName), TypeHistory.Push(type));

        public string MemberPath => string.Join(".", Members);
    }

    internal static class AutoGenHandlerExtensions
    {
        public static IGen CreateGen(this IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context) =>
            innerHandler.CreateGen(innerHandler, type, context);
    }
}
