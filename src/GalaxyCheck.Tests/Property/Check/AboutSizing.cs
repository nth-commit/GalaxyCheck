using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;
using FsCheck.Xunit;
using System;
using Xunit;

namespace Tests.Property.Check
{
    [Properties(MaxTest = 10, Arbitrary = new [] { typeof(ArbitrarySize) })]
    public class AboutSizing
    {
        [Property]
        public void ItFalsifiesAPropertyThatOnlyFalsifiesAtSmallerSizes(Size size) => TestWithSeed(seed =>
        {
            var property = GC.Gen.Int32().Between(0, 100).ForAll(x => x > 50);

            PropertyAssert.Falsifies(property, seed, size.Value);
        });

        [Property]
        public void ItFalsifiesAPropertyThatOnlyFalsifiesAtLargerSizes() => TestWithSeed(seed =>
        {
            var property = GC.Gen.Int32().Between(0, 100).ForAll(x => x < 50);

            PropertyAssert.Falsifies(property, seed);
        });

        [Property]
        public void WhenGivenASize_AndTheGenIsNotSelfResizing_ThePropertyIsNotResized(Size size) => TestWithSeed(seed =>
        {
            var property = GC.Gen.Int32().ForAll(_ => true);

            var result = property.Check(seed: seed, size: size.Value);

            Assert.Equal(size.Value, result.NextParameters.Size.Value);
        });
    }
}
