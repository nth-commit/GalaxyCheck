using FsCheck;
using FsCheck.Xunit;
using System;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.ListGen
{
    public class AboutDistribution
    {
        [Property]
        public FsCheck.Property ItHasADistributionOverLengthLikeInt32Gen(int x, int y, GC.Gen.Bias bias, object element)
        {
            // The randomness is controlled in this test by using a constant generator for the elements. Because a
            // constant generator does not consume randomness, the int gen and the list gen will stay in sync with
            // their randomness consumption, meaning they will produce the same values.

            Action test = () => TestWithSeed(seed =>
            {
                var int32Gen = GC.Gen.Int32().Between(x, y).WithBias(bias).NoShrink();
                var elementGen = GC.Gen.Constant(element);
                var listGen = elementGen.ListOf().BetweenLengths(x, y).WithLengthBias(bias).NoShrink();

                GenAssert.ValuesEqual(
                    int32Gen,
                    listGen.Select(l => l.Count),
                    seed);
            });

            return test.When(x >= 0 && x <= 100 && y >= 0 && y <= 100);
        }
    }
}
