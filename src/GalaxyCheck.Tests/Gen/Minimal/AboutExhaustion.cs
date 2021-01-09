using FsCheck.Xunit;
using System;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;
using Xunit;

namespace Tests.Gen.Minimal
{
    [Properties(Arbitrary = new [] { typeof(ArbitraryGen) }, MaxTest = 10)]
    public class AboutExhaustion
    {
        [Property]
        public void ItExhaustsWithAnImpossiblePredicate(GC.Abstractions.IGen<object> gen0)
        {
            var gen = gen0.Where(_ => false);

            TestWithSeed(seed =>
            {
                Action test = () => gen.Minimal(new RunConfig(seed: seed));

                Assert.Throws<GenExhaustionException>(test);
            });
        }
    }
}
