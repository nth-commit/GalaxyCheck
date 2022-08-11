using System;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class NullableGenHandler : IReflectedGenHandler
    {
        public bool CanHandleGen(Type type, ReflectedGenHandlerContext context) =>
            type.IsNullableStruct() || (context.NullabilityInfo != null && IsNullable(context.NullabilityInfo));

        public IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context)
        {
            if (type.IsNullableStruct())
            {
                var nonNullType = type.GetGenericArguments().Single();

                var nullableStructMethodInfo = typeof(Gen).GetMethod(
                    nameof(Gen.NullableStruct),
                    BindingFlags.Static | BindingFlags.Public)!;

                return DecorateGenWithNullability(innerHandler, context, nonNullType, nullableStructMethodInfo);
            }
            else
            {
                var nullableMethodInfo = typeof(Gen).GetMethod(
                    nameof(Gen.Nullable),
                    BindingFlags.Static | BindingFlags.Public)!;

                return DecorateGenWithNullability(innerHandler, context, type, nullableMethodInfo);
            }
        }

        private static IGen DecorateGenWithNullability(
            IReflectedGenHandler innerHandler,
            ReflectedGenHandlerContext context,
            Type nonNullType,
            MethodInfo nullableGenMethodInfo)
        {
            var nonNullGen = innerHandler.CreateGen(nonNullType, context.SuppressNullability());

            var genericMethodInfo = nullableGenMethodInfo.MakeGenericMethod(nonNullType);

            return (IGen)genericMethodInfo.Invoke(null!, new object[] { nonNullGen })!;
        }

        private static bool IsNullable(NullabilityInfo nullabilityInfo) =>
            nullabilityInfo.ReadState == NullabilityState.Nullable && nullabilityInfo.WriteState == NullabilityState.Nullable;
    }
}
