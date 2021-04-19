namespace GalaxyCheck
{
    using GalaxyCheck.Gens.Enum;
    using System;
    using System.Linq;
    using System.Reflection;

    public static partial class Gen
    {
        public static IGen<T> Enum<T>()
            where T : struct
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                return Advanced.Error<T>("EnumGen", $"Type '{type}' is not an enum");
            }

            var isFlagsEnum = type.GetCustomAttributes<FlagsAttribute>().Any();
            return isFlagsEnum
                ? EnumGenHelpers.GenFlagsEnum<T>()
                : EnumGenHelpers.GenNonFlagsEnum<T>();
        }
    }
}

namespace GalaxyCheck.Gens.Enum
{
    using System.Linq;

    internal static class EnumGenHelpers
    {
        // TODO: This will shrink more efficiently in the future if we use Gen.Set (currently non-existent API). Then,
        // we won't need to witness the shrinks for duplicate enum values that get erased in the aggregation.
        public static IGen<T> GenFlagsEnum<T>() => Gen
            .Element(System.Enum.GetValues(typeof(T)).Cast<int>())
            .ListOf()
            .WithCountGreaterThan(0)
            .Select(xs => xs.Aggregate(0, (acc, curr) => acc | curr))
            .Cast<T>();

        public static IGen<T> GenNonFlagsEnum<T>() => Gen.Element(System.Enum.GetValues(typeof(T)).Cast<T>());
    }
}
