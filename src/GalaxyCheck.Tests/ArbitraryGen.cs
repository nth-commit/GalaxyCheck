using FsCheck;
using GalaxyCheck;
using GC = GalaxyCheck;

namespace Tests
{
    public static class ArbitraryGen
    {
        public static Arbitrary<IGen<object>> Gen() => FsCheck.Gen.Elements(
            GC.Gen.Constant(false).Cast<object>(),
            GC.Gen.Int32().Cast<object>()).ToArbitrary();
    }
}
