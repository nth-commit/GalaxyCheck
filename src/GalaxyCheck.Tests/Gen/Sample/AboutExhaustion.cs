extern alias GalaxyCheckSample;
using FsCheck.Xunit;
using System;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Sample
{
    [Properties(Arbitrary = new [] { typeof(ArbitraryGen) }, MaxTest = 10)]
    public class AboutExhaustion
    {
        [Property]
        public void ItExhaustsWithAnImpossiblePredicate(IGen<object> gen0)
        {
            var gen = gen0.Where(_ => false);

            TestWithSeed(seed =>
            {
                Action throwing = () => gen.Sample(seed: seed);

                Assert.Throws<GC.Exceptions.GenExhaustionException>(throwing);
            });
        }
    }
}
