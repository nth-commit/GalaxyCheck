using System;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenHandlers
{
    internal class ArrayAutoGenHandler : IAutoGenHandler
    {
        public bool CanHandleGen(Type type, AutoGenHandlerContext context) => type.IsArray;

        public IGen CreateGen(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context)
        {
            var elementType = type.GetElementType();
            var elementGen = innerHandler.CreateGen(elementType, context);

            var methodInfo = typeof(ArrayAutoGenHandler).GetMethod(
                nameof(CreateArrayGen),
                BindingFlags.Static | BindingFlags.NonPublic)!;

            var genericMethodInfo = methodInfo.MakeGenericMethod(elementType);

            return (IGen)genericMethodInfo.Invoke(null!, new object[] { elementGen });
        }

        private static IGen<T[]> CreateArrayGen<T>(IGen<T> elementGen) => elementGen.ListOf().Select(x => x.ToArray());
    }
}
