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
        [Property(Arbitrary = new[] { typeof(ArbitraryGen) }, EndSize = 50)]
        public FsCheck.Property ItConsumesRandomnessLikeSample(Iterations iterations, int randomnessConsumptionPerIteration, object value)
        {
            var gen = GC.Gen.Advanced.Create<object>((useNextInt, _) =>
            {
                for (int i = 0; i < randomnessConsumptionPerIteration; i++)
                {
                    useNextInt(0, 0);
                }

                return value;
            });

            Action test = () => TestWithSeed(seed =>
            {
                var property = gen.ForAll(_ => true);

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations.Value, seed: seed);
                var check = property.Check(iterations: iterations.Value, seed: seed);

                Assert.Equal(sample.RandomnessConsumption, check.RandomnessConsumption);
            });

            return test.When(iterations.Value > 0);
        }
    }
}
