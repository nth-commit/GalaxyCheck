using System;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class NullableStructGenHandler : IReflectedGenHandler
    {
        public bool CanHandleGen(Type type, ReflectedGenHandlerContext context) => type.IsNullableStruct();

        public IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context)
        {
            var nonNullType = type.GetGenericArguments().Single();

            var innerGen = innerHandler.CreateGen(nonNullType, context);

            var methodInfo = typeof(Gen).GetMethod(
                nameof(Gen.NullableStruct),
                BindingFlags.Static | BindingFlags.Public)!;
            var genericMethodInfo = methodInfo.MakeGenericMethod(nonNullType);

            return (IGen)genericMethodInfo.Invoke(null!, new object[] { innerGen })!;
        }
    }
}
