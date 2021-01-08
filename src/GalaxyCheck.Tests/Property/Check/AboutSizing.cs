using FsCheck;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;
using FsCheck.Xunit;

namespace Tests.Property.Check
{
    [Properties(Arbitrary = new [] { typeof(ArbitrarySize) })]
    public class AboutSizing
    {
        [Property]
        public void ItFalsifiesAPropertyThatOnlyFalsifiesAtSmallerSizes(GC.Abstractions.ISize size) => TestWithSeed(seed =>
        {
            var property = GC.Gen.Int32().Between(0, 100).ToProperty(x => x > 50);

            PropertyAssert.Falsifies(property, seed, size);
        });

        [Property]
        public void ItFalsifiesAPropertyThatOnlyFalsifiesAtLargerSizes(GC.Abstractions.ISize size) => TestWithSeed(seed =>
        {
            var property = GC.Gen.Int32().Between(0, 100).ToProperty(x => x < 50);

            PropertyAssert.Falsifies(property, seed, size);
        });
    }
}
