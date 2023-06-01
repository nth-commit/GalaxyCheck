using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GalaxyCheck.Gens.ReflectedGenHelpers
{
    internal record ReflectedGenHandlerContext(
        ImmutableList<string> Members,
        ImmutableStack<Type> TypeHistory,
        NullabilityInfo? NullabilityInfo)
    {
        public static ReflectedGenHandlerContext Create(Type type, NullabilityInfo? nullabilityInfo) =>
            new ReflectedGenHandlerContext(ImmutableList.Create("$"), ImmutableStack.Create(type), nullabilityInfo);

        public ReflectedGenHandlerContext Next(string memberName, Type type, NullabilityInfo? nullabilityInfo) =>
            new ReflectedGenHandlerContext(Members.Add(memberName), TypeHistory.Push(type), nullabilityInfo);

        public ReflectedGenHandlerContext SuppressNullability() => this with
        {
            NullabilityInfo = null
        };

        public string MemberPath => string.Join(".", Members);
    }

    internal static class ReflectedGenHandlerContextExtensions
    {
        public static int CalculateStableSeed(this ReflectedGenHandlerContext context) => Encoding.Unicode
            .GetBytes(context.MemberPath)
            .Aggregate(0, (acc, curr) =>
            {
                unchecked
                {
                    return acc + curr;
                }
            });

        public static IGen<T> Error<T>(this ReflectedGenHandlerContext context, string message)
        {
            var suffix = context.Members.Count == 1 ? "" : $" at path '{context.MemberPath}'";
            return Gen.Advanced.Error<T>(message + suffix);
        }

        public static IGen Error(this ReflectedGenHandlerContext context, Type type, string message)
        {
            var suffix = context.Members.Count == 1 ? "" : $" at path '{context.MemberPath}'";
            return Gen.Advanced.Error(type, message + suffix);
        }
    }
}
