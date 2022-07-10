using System;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class ArrayReflectedGenHandler : IReflectedGenHandler
    {
        public bool CanHandleGen(Type type, ReflectedGenHandlerContext context) => type.IsArray;

        public IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context)
        {
            var elementType = type.GetElementType()!;
            var elementGen = innerHandler.CreateGen(elementType, context);

            var methodInfo = typeof(ArrayReflectedGenHandler).GetMethod(
                nameof(CreateArrayGen),
                BindingFlags.Static | BindingFlags.NonPublic)!;

            var genericMethodInfo = methodInfo.MakeGenericMethod(elementType);

            return (IGen)genericMethodInfo.Invoke(null!, new object[] { elementGen })!;
        }

        private static IGen<T[]> CreateArrayGen<T>(IGen<T> elementGen) => elementGen.ListOf().Select(x => x.ToArray());
    }
}
