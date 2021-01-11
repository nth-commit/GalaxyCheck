using FsCheck;
using FsCheck.Xunit;
using System;
using Xunit;
using S = GalaxyCheck.Internal.Sizing;

namespace Tests.Sizing.BoundsScalingFunc
{
    [Properties(Arbitrary = new [] { typeof(ArbitrarySize) })]
    public class AboutScaledLinearly
    {
        [Property]
        public FsCheck.Property WhenSizeIsMin_ItReturnsTheOriginAsTheBounds(int min, int max, int origin)
        {
            Action test = () =>
            {
                var f = S.BoundsScalingFactoryFuncs.ScaledLinearly(min, max, origin);

                var bounds = f(S.Size.MinValue);

                Assert.Equal((origin, origin), bounds);
            };

            return test.When(min <= origin && origin <= max);
        }

        [Property]
        public FsCheck.Property WhenSizeIsMax_ItAlwaysReturnsTheGivenBounds(int min, int max, int origin)
        {
            Action test = () =>
            {
                var f = S.BoundsScalingFactoryFuncs.ScaledLinearly(min, max, origin);

                var bounds = f(GalaxyCheck.Internal.Sizing.Size.MaxValue);

                Assert.Equal((min, max), bounds);
            };

            return test.When(min <= origin && origin <= max);
        }

        [Property]
        public void WhenMinAndMaxAreOrigin_ItAlwaysReturnsTheGivenBounds(int origin, Tests.Size size)
        {
            var f = S.BoundsScalingFactoryFuncs.ScaledLinearly(origin, origin, origin);

            var bounds = f(new S.Size(size.Value));

            Assert.Equal((origin, origin), bounds);
        }

        [Theory]
        [InlineData(0, 10, 0, 50, 0, 5)] 
        [InlineData(10, 0, 0, 50, 5, 0)]
        [InlineData(-10, 10, 0, 50, -5, 5)]
        public void Examples(int min, int max, int origin, int size, int scaledMin, int scaledMax)
        {
            var f = S.BoundsScalingFactoryFuncs.ScaledLinearly(min, max, origin);

            var bounds = f(new S.Size(size));

            Assert.Equal(bounds, (scaledMin, scaledMax));
        }
    }
}
