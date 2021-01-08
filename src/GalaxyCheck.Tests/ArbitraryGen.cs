using FsCheck;
using GalaxyCheck;
using GC = GalaxyCheck;


namespace Tests
{
    public static class ArbitraryGen
    {
        public static Arbitrary<GC.Abstractions.IGen<object>> Gen() => FsCheck.Gen.Elements(
            GC.Gen.Constant(false).Select(x => x as object),
            GC.Gen.Int32().Select(x => x as object)).ToArbitrary();
    }
}
