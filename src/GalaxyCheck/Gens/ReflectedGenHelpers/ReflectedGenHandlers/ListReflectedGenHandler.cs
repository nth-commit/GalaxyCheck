﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class ListReflectedGenHandler : IReflectedGenHandler
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

            var methodInfo = typeof(ListReflectedGenHandler).GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic)!;

            var genericMethodInfo = methodInfo.MakeGenericMethod(elementType);

            return (IGen)genericMethodInfo.Invoke(null!, new object[] { elementGen });
        }

        private static readonly IReadOnlyDictionary<Type, string> GenMethodByGenericTypeDefinition = new Dictionary<Type, string>
        {
            { typeof(IReadOnlyCollection<>), nameof(CreateListGen) },
            { typeof(IReadOnlyList<>), nameof(CreateListGen) },
            { typeof(List<>), nameof(CreateListGen) },
            { typeof(ImmutableList<>), nameof(CreateImmutableListGen) },
            { typeof(IList<>), nameof(CreateListGen) }
        };

        private static IGen<IReadOnlyList<T>> CreateIReadOnlyListGen<T>(IGen<T> elementGen) => elementGen.ListOf();

        private static IGen<List<T>> CreateListGen<T>(IGen<T> elementGen) => CreateIReadOnlyListGen(elementGen).Select(x => x.ToList());

        private static IGen<ImmutableList<T>> CreateImmutableListGen<T>(IGen<T> elementGen) => CreateIReadOnlyListGen(elementGen).Select(x => x.ToImmutableList());
    }
}
