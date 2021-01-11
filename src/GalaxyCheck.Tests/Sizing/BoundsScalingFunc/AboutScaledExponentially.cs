using FsCheck;
using FsCheck.Xunit;
using System;
using Xunit;
using S = GalaxyCheck.Internal.Sizing;

namespace Tests.Sizing.BoundsScalingFunc
{
    [Properties(Arbitrary = new[] { typeof(ArbitrarySize) })]
    public class AboutScaledExponentially
    {
        [Property]
        public FsCheck.Property WhenSizeIsMin_ItReturnsTheOriginAsTheBounds(int min, int max, int origin)
        {
            Action test = () =>
            {
                var f = S.BoundsScalingFactoryFuncs.ScaledExponentially(min, max, origin);

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
                var f = S.BoundsScalingFactoryFuncs.ScaledExponentially(min, max, origin);

                var bounds = f(S.Size.MaxValue);

                Assert.Equal((min, max), bounds);
            };

            return test.When(min <= origin && origin <= max);
        }

        [Property]
        public void WhenMinAndMaxAreOrigin_ItAlwaysReturnsTheGivenBounds(int origin, Tests.Size size)
        {
            var f = S.BoundsScalingFactoryFuncs.ScaledExponentially(origin, origin, origin);

            var bounds = f(new S.Size(size.Value));

            Assert.Equal((origin, origin), bounds);
        }

        [Theory]
        [InlineData(1, -1, 1)]
        [InlineData(10, -2, 2)]
        [InlineData(50, -10, 10)]
        [InlineData(75, -32, 32)]
        [InlineData(99, -95, 95)]
        public void ExamplesForARangeFromNegative100ToPositive100AndOrigin0(int size, int expectedMin, int expectedMax)
        {
            var f = S.BoundsScalingFactoryFuncs.ScaledExponentially(-100, 100, 0);

            var bounds = f(new S.Size(size));

            Assert.Equal((expectedMin, expectedMax), bounds);
        }
    }
}
