﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories
{
    internal class ListAutoGenFactory : IAutoGenFactory
    {
        public bool CanHandleType(Type type, AutoGenFactoryContext context)
        {
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                return GenMethodByGenericTypeDefinition.ContainsKey(genericTypeDefinition);
            }

            return false;
        }

        public IGen CreateGen(IAutoGenFactory innerFactory, Type type, AutoGenFactoryContext context)
        {
            var elementType = type.GetGenericArguments().Single();
            var elementGen = innerFactory.CreateGen(elementType, context);

            var genericTypeDefinition = type.GetGenericTypeDefinition();
            var methodName = GenMethodByGenericTypeDefinition[genericTypeDefinition];

            var methodInfo = typeof(ListAutoGenFactory).GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic)!;

            var genericMethodInfo = methodInfo.MakeGenericMethod(elementType);

            return (IGen)genericMethodInfo.Invoke(null!, new object[] { elementGen });
        }

        private static readonly IReadOnlyDictionary<Type, string> GenMethodByGenericTypeDefinition = new Dictionary<Type, string>
            {
                { typeof(IReadOnlyList<>), nameof(CreateListGen) },
                { typeof(List<>), nameof(CreateListGen) },
                { typeof(ImmutableList<>), nameof(CreateImmutableListGen) },
                { typeof(IList<>), nameof(CreateListGen) },
            };

        private static IGen<List<T>> CreateListGen<T>(IGen<T> elementGen) => CreateImmutableListGen(elementGen).Select(x => x.ToList());

        private static IGen<ImmutableList<T>> CreateImmutableListGen<T>(IGen<T> elementGen) => elementGen.ListOf();
    }
}
