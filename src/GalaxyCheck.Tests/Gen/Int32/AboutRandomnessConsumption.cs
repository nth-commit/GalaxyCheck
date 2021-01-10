using FsCheck;
using FsCheck.Xunit;
using System;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Int32
{
    [Properties(Arbitrary = new [] { typeof(ArbitraryIterations) })]
    public class AboutRandomnessConsumption
    {
        [Property]
        public FsCheck.Property ItConsumesRandomnessOncePerIteration(int min, int max, int origin, GC.Gen.Bias bias, Iterations iterations)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32()
                    .GreaterThanEqual(min)
                    .LessThanEqual(max)
                    .ShrinkTowards(origin)
                    .WithBias(bias);

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations.Value, seed: seed);

                Assert.Equal(iterations.Value, sample.RandomnessConsumption);
            });

            return test.When(iterations.Value > 0 && min <= origin && origin <= max);
        }
    }
}
