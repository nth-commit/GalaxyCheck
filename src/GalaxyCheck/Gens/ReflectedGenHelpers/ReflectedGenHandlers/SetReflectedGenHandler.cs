using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class SetReflectedGenHandler : IReflectedGenHandler
    {
        public bool CanHandleGen(Type type, ReflectedGenHandlerContext context)
        {
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                return GenMethodByGenericTypeDefinition.ContainsKey(genericTypeDefinition);
            }

            return false;
        }

        public IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context)
        {
            var elementType = type.GetGenericArguments().Single();
            var elementGen = innerHandler.CreateGen(elementType, context);

            var genericTypeDefinition = type.GetGenericTypeDefinition();
            var methodName = GenMethodByGenericTypeDefinition[genericTypeDefinition];

            var methodInfo = typeof(SetReflectedGenHandler).GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic)!;

            var genericMethodInfo = methodInfo.MakeGenericMethod(elementType);

            return (IGen)genericMethodInfo.Invoke(null!, new object[] { elementGen });
        }

        private static readonly IReadOnlyDictionary<Type, string> GenMethodByGenericTypeDefinition = new Dictionary<Type, string>
        {
            { typeof(HashSet<>), nameof(CreateHashSetGen) },
            { typeof(ISet<>), nameof(CreateHashSetGen) },
            { typeof(ImmutableHashSet<>), nameof(CreateImmutableHashSetGen) },
        };

        private static IGen<HashSet<T>> CreateHashSetGen<T>(IGen<T> elementGen) => elementGen.SetOf().Select(s => new HashSet<T>(s));

        private static IGen<ImmutableHashSet<T>> CreateImmutableHashSetGen<T>(IGen<T> elementGen) => CreateHashSetGen(elementGen).Select(x => x.ToImmutableHashSet());
    }
}
