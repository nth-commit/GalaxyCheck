using FsCheck.Xunit;
using System;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Minimal
{
    [Properties(Arbitrary = new [] { typeof(ArbitraryGen) }, MaxTest = 10)]
    public class AboutExhaustion
    {
        [Property]
        public void ItExhaustsWithAnImpossiblePredicate(GC.IGen<object> gen0)
        {
            var gen = gen0.Where(_ => false);

            TestWithSeed(seed =>
            {
                Action test = () => gen.Minimal(seed: seed);

                Assert.Throws<GC.Exceptions.GenExhaustionException>(test);
            });
        }
    }
}
