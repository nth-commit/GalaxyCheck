using FsCheck;
using FsCheck.Xunit;
using System;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Property.Check
{
    [Properties(Arbitrary = new [] { typeof(ArbitraryIterations) })]
    public class AboutRandomnessConsumption
    {
        public static class ArbitraryGen
        {
            public static Arbitrary<GC.Abstractions.IGen<object>> Gen() => FsCheck.Gen.Elements(
                GC.Gen.Constant(false).Select(x => x as object),
                GC.Gen.Int32().Select(x => x as object)).ToArbitrary();
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryGen) })]
        public FsCheck.Property ItConsumesRandomnessLikeSample(Iterations iterations, GC.Abstractions.IGen<object> gen)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var property = GC.Property.ForAll(gen, _ => true);
                var config = new RunConfig(iterations: iterations.Value, seed: seed);

                var sample = gen.Advanced.SampleWithMetrics(config);
                var check = property.Check(config);

                Assert.Equal(sample.RandomnessConsumption, check.RandomnessConsumption);
            });

            return test.When(iterations.Value > 0);
        }
    }
}
