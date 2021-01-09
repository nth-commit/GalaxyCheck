using FsCheck.Xunit;
using System;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;
using Xunit;

namespace Tests.Property.Check
{
    [Properties(Arbitrary = new[] { typeof(ArbitraryGen) }, MaxTest = 10)]
    public class AboutExhaustion
    {
        [Property]
        public void ItExhaustsWithAnImpossiblePredicate(GC.Abstractions.IGen<object> gen)
        {
            var property = gen.Where(_ => false).ToProperty(x => true);

            TestWithSeed(seed =>
            {
                Action test = () => property.Check(new RunConfig(seed: seed));

                Assert.Throws<GenExhaustionException>(test);
            });
        }
    }
}
