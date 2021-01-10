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
        public void ItFalsifiesAPropertyThatOnlyFalsifiesAtSmallerSizes(Size size) => TestWithSeed(seed =>
        {
            var property = GC.Gen.Int32().Between(0, 100).ForAll(x => x > 50);

            PropertyAssert.Falsifies(property, seed, size.Value);
        });

        [Property]
        public void ItFalsifiesAPropertyThatOnlyFalsifiesAtLargerSizes(Size size) => TestWithSeed(seed =>
        {
            var property = GC.Gen.Int32().Between(0, 100).ForAll(x => x < 50);

            PropertyAssert.Falsifies(property, seed, size.Value);
        });
    }
}
