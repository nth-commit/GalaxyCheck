using FsCheck.Xunit;
using System;
using System.Linq;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Infinite
{
    [Properties(Arbitrary = new[] { typeof(ArbitraryGen) }, MaxTest = 10)]
    public class AboutExhaustion
    {
        [Property]
        public void ItExhaustsWithAnImpossiblePredicate(IGen<object> elementGen)
        {
            var gen = elementGen.Where(_ => false).InfiniteOf();

            TestWithSeed(seed =>
            {
                Action throwing = () => gen.SampleOne(seed: seed).Take(1).ToList();

                Assert.Throws<GC.Exceptions.GenExhaustionException>(throwing);
            });
        }
    }
}
