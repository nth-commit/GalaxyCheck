using FsCheck;
using FsCheck.Xunit;
using System;
using Xunit;
using S = GalaxyCheck.Sizing;

namespace Tests.Sizing.BoundsScalingFunc
{
    [Properties(Arbitrary = new [] { typeof(ArbitrarySize) })]
    public class AboutUnscaled
    {
        [Property]
        public FsCheck.Property ItAlwaysReturnsTheGivenBounds(int min, int max, int origin, S.Size size)
        {
            Action test = () =>
            {
                var f = S.BoundsScalingFactoryFuncs.Unscaled(min, max, origin);

                var bounds = f(size);

                Assert.Equal((min, max), bounds);
            };

            return test.When(min <= origin && origin <= max);
        }
    }
}
