using GalaxyCheck.Stable;

namespace GalaxyCheck.Tests;

public static class DomainGen
{
    public static class TestFunctions
    {
        public static Stable.IGen<Func<bool>> Nullary() => Stable.Gen.Function(Stable.Gen.Boolean());

        public static Stable.IGen<Func<T, bool>> Unary<T>() => Stable.Gen.Function<T, bool>(Stable.Gen.Boolean());
    }

    public static class Gens
    {
        public static Stable.IGen<IGen<object>> Error() => Stable.Gen
            .String()
            .WithLengthBetween(0, 10)
            .Select(Gen.Advanced.Error<object>);
    }

    public static class Properties
    {
        public static Stable.IGen<PropertyProxy> Any() =>
            from seed in Seed()
            from size in Size()
            from testFunction in TestFunctions.Unary<object>()
            select new PropertyProxy(seed, size, testFunction);

        public static Stable.IGen<Property> Nullary() =>
            from testFunction in TestFunctions.Nullary()
            select Property.Nullary(testFunction);
    }

    public static Stable.IGen<int> Size() =>
        Stable.Gen.Int32().Between(GalaxyCheck.Gens.Parameters.Size.Zero.Value, GalaxyCheck.Gens.Parameters.Size.Max.Value);

    public static Stable.IGen<int> Seed() => Stable.Gen.Int32().NoShrink();

    public static Stable.IGen<int?> SeedWaypoint() => Seed().OrNullStruct();

    public static Stable.IGen<Property> ToProperty(this Stable.IGen<IGen<object>> metaGen) =>
        from gen in metaGen
        from testFunction in TestFunctions.Unary<object>()
        select new Property((IGen<Property.Test<object>>)Property.ForAll<object>(gen, testFunction));

    public class SeedAttribute : Stable.GenAttribute
    {
        public override Stable.IGen Value => Seed();
    }
}
