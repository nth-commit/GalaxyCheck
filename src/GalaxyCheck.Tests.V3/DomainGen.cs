using NebulaCheck;

namespace GalaxyCheck_Tests_V3;

public static class DomainGen
{
    public static class TestFunctions
    {
        public static IGen<Func<bool>> Nullary() => Gen.Function(Gen.Boolean());

        public static IGen<Func<T, bool>> Unary<T>() => Gen.Function<T, bool>(Gen.Boolean());
    }

    public static class Gens
    {
        public static IGen<GalaxyCheck.IGen<object>> Error() => Gen
            .String()
            .WithLengthBetween(0, 10)
            .Select(GalaxyCheck.Gen.Advanced.Error<object>);
    }

    public static class Properties
    {
        public static IGen<PropertyProxy> FromFunction(Func<bool> testFunction) =>
            from seed in Seed()
            from size in Size()
            select new PropertyProxy(seed, size, _ => testFunction());

        public static IGen<PropertyProxy> FromFunction(Func<object, bool> testFunction) =>
            from seed in Seed()
            from size in Size()
            select new PropertyProxy(seed, size, testFunction);

        public static IGen<PropertyProxy> Any() =>
            from seed in Seed()
            from size in Size()
            from testFunction in TestFunctions.Unary<object>()
            select new PropertyProxy(seed, size, testFunction);

        public static IGen<GalaxyCheck.Property> Nullary() =>
            from testFunction in TestFunctions.Nullary()
            select GalaxyCheck.Property.Nullary(testFunction);
    }

    public static IGen<int> Size() => Gen.Int32().Between(GalaxyCheck.Gens.Parameters.Size.Zero.Value, GalaxyCheck.Gens.Parameters.Size.Max.Value);

    public static IGen<int> Seed() => Gen.Int32();

    public static IGen<int?> SeedWaypoint() => Seed().OrNullStruct();

    public static IGen<GalaxyCheck.Property> ToProperty(this IGen<GalaxyCheck.IGen<object>> metaGen) =>
        from gen in metaGen
        from testFunction in TestFunctions.Unary<object>()
        select new GalaxyCheck.Property((GalaxyCheck.IGen<GalaxyCheck.Property.Test<object>>)GalaxyCheck.Property.ForAll<object>(gen, testFunction));
}
