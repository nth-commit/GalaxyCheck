using System;
using System.Reflection;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal static class GenReflectionExtensions
    {
        public static IGen<object?> MaybeNullableByNullabilityInfo(
            this IGen<object> gen,
            NullabilityInfo? nullabilityInfo,
            Type declaredType)
        {
            if (nullabilityInfo == null) return gen;

            if (declaredType.IsValueType || declaredType.IsNullableStruct())
            {
                // Nullable structs are handled more broadly by NullableStructGenHandler
                return gen;
            }

            if (nullabilityInfo.ReadState != NullabilityState.Nullable)
            {
                return gen;
            }

            return Gen.Nullable(gen);
        }

        public static IGen MaybeNullableByNullabilityInfo(
            this IGen gen,
            NullabilityInfo? nullabilityInfo,
            Type declaredType)
        {
            if (nullabilityInfo == null) return gen;

            if (declaredType.IsValueType || declaredType.IsNullableStruct())
            {
                // Nullable structs are handled more broadly by NullableStructGenHandler
                return gen;
            }

            if (nullabilityInfo.ReadState != NullabilityState.Nullable)
            {
                return gen;
            }

            var nullableMethodInfo = typeof(Gen).GetMethod(
                nameof(Gen.Nullable),
                BindingFlags.Static | BindingFlags.Public)!;

            var genericNullableMethodInfo = nullableMethodInfo.MakeGenericMethod(declaredType);

            return (IGen)genericNullableMethodInfo.Invoke(null!, new object[] { gen })!;
        }
    }
}
