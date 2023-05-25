namespace GalaxyCheck.Tests.TestUtility;

public static class DummyTestFunctions
{
    public static class Int32
    {
        public static Func<int, bool> NonZero() => x => x != 0;
    }

    public static class Size
    {
        public static Func<GalaxyCheck.Gens.Parameters.Size, bool> IsGreaterThan(GalaxyCheck.Gens.Parameters.Size value) =>
            size => size.Value > value.Value;

        public static Func<GalaxyCheck.Gens.Parameters.Size, bool> Top50thPercentile() =>
            IsGreaterThan(new GalaxyCheck.Gens.Parameters.Size(GalaxyCheck.Gens.Parameters.Size.Max.Value / 2));
    }
}
