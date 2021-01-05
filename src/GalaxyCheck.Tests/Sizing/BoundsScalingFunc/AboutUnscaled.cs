using FsCheck;
using FsCheck.Xunit;
using GalaxyCheck.Abstractions;
using System;
using Xunit;
using S = GalaxyCheck.Sizing;

namespace GalaxyCheck.Tests.Sizing.BoundsScalingFunc
{
    [Properties(Arbitrary = new [] { typeof(ArbitrarySize) })]
    public class AboutUnscaled
    {
        [Property]
        public Property ItAlwaysReturnsTheGivenBounds(int min, int max, int origin, ISize size)
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
