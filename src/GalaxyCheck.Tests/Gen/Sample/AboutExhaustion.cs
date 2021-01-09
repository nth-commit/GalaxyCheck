﻿using FsCheck.Xunit;
using System;
using GalaxyCheck;
using static Tests.TestUtils;
using Xunit;

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
                Action test = () => gen.Sample(new RunConfig(seed: seed));

                Assert.Throws<GenExhaustionException>(test);
            });
        }
    }
}
