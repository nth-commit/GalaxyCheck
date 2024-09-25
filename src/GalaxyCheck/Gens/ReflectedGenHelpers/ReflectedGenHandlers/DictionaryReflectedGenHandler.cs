using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class DictionaryReflectedGenHandler : IReflectedGenHandler
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
            var keyType = type.GetGenericArguments().First();
            var elementType = type.GetGenericArguments().Skip(1).First();
            
            var elementNullabilityInfo = context.NullabilityInfo?.GenericTypeArguments.Skip(1).SingleOrDefault();
            var keyGen = innerHandler.CreateGen(keyType, context.Next("[*]", keyType, null));
            var elementGen = innerHandler.CreateGen(elementType, context.Next("[*]", elementType, elementNullabilityInfo));

            var genericTypeDefinition = type.GetGenericTypeDefinition();
            var methodName = GenMethodByGenericTypeDefinition[genericTypeDefinition];

            var methodInfo = typeof(DictionaryReflectedGenHandler).GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic)!;

            var genericMethodInfo = methodInfo.MakeGenericMethod(keyType, elementType);

            return (IGen)genericMethodInfo.Invoke(null!, new object[] { keyGen, elementGen })!;
        }

        private static readonly IReadOnlyDictionary<Type, string> GenMethodByGenericTypeDefinition = new Dictionary<Type, string>
        {
            { typeof(Dictionary<,>), nameof(CreateDictionaryGen) },
        };

        private static IGen<Dictionary<TKey, TValue>> CreateDictionaryGen<TKey, TValue>(IGen<TKey> keyGen, IGen<TValue> elementGen) where TKey : notnull
        {
            return Gen
                .Int32()
                .Between(0, 2)
                .SelectMany(count => Gen.Zip(keyGen.SetOf().WithCount(count), elementGen.ListOf().WithCount(count)))
                .Select(keysAndValues => Enumerable.Zip(keysAndValues.Item1, keysAndValues.Item2).ToDictionary(x => x.First, x => x.Second));
        }
    }
}
