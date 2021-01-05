using FsCheck;
using GalaxyCheck.Abstractions;
using S = GalaxyCheck.Sizing;

namespace Tests
{
    public static class ArbitrarySize
    {
        public static Arbitrary<ISize> Size() => FsCheck.Gen.Choose(0, 100).Select<int, ISize>(x => new S.Size(x)).ToArbitrary();

    }
}
