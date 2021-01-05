using FsCheck.Xunit;
using System;
using Xunit;
using GalaxyCheck.Aggregators;
using G = GalaxyCheck.Gen;
using static GalaxyCheck.Tests.TestUtils;
using FsCheck;

namespace GalaxyCheck.Tests.Gen.Int32
{
    public class AboutRandomnessConsumption
    {
        [Property]
        public Property ItConsumesRandomnessOncePerIteration(int min, int max, int origin, G.Bias bias)
        {
            var iterations = 10;

            Action test = () => TestWithSeed(seed =>
            {
                var gen = G.Int32()
                    .GreaterThanEqual(min)
                    .LessThanEqual(max)
                    .ShrinkTowards(origin)
                    .WithBias(bias);

                var sample = gen.SampleWithMetrics(opts => opts.WithSeed(seed).WithIterations(iterations));

                Assert.Equal(iterations, sample.RandomnessConsumption);
            });

            return test.When(min <= origin && origin <= max);
        }
    }
}
