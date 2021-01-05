using FsCheck;
using GalaxyCheck.Abstractions;
using S = GalaxyCheck.Sizing;

namespace GalaxyCheck.Tests
{
    public static class ArbitrarySize
    {
        public static Arbitrary<ISize> Size() => FsCheck.Gen.Choose(0, 99).Select<int, ISize>(x => new S.Size(x)).ToArbitrary();

    }
}
