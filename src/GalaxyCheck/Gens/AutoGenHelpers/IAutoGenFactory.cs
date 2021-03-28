using System;
using System.Collections.Immutable;

namespace GalaxyCheck.Gens.AutoGenHelpers
{
    internal interface IAutoGenFactory
    {
        bool CanHandleType(Type type, AutoGenFactoryContext context);

        IGen CreateGen(IAutoGenFactory innerFactory, Type type, AutoGenFactoryContext context);
    }

    internal record AutoGenFactoryContext(
        ImmutableList<string> Members,
        ImmutableStack<Type> TypeHistory)
    {
        public static AutoGenFactoryContext Create(Type type) =>
            new AutoGenFactoryContext(ImmutableList.Create("$"), ImmutableStack.Create(type));

        public AutoGenFactoryContext Next(string memberName, Type type) =>
            new AutoGenFactoryContext(Members.Add(memberName), TypeHistory.Push(type));

        public string MemberPath => string.Join(".", Members);
    }

    internal static class AutoGenFactoryExtensions
    {
        public static IGen CreateGen(this IAutoGenFactory innerFactory, Type type, AutoGenFactoryContext context) =>
            innerFactory.CreateGen(innerFactory, type, context);
    }
}
