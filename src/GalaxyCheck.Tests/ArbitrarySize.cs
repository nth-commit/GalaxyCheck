using FsCheck;
using GC = GalaxyCheck;

namespace Tests
{
    public static class ArbitrarySize
    {
        public static Arbitrary<GC.Sizing.Size> Size() => FsCheck.Gen.Choose(0, 100).Select(x => new GC.Sizing.Size(x)).ToArbitrary();

    }
}
