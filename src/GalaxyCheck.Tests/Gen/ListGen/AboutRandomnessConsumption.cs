using FsCheck;
using FsCheck.Xunit;
using System;
using System.Linq;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.ListGen
{
    [Properties(Arbitrary = new[] { typeof(ArbitraryIterations) })]
    public class AboutRandomnessConsumption
    {
        [Property]
        public FsCheck.Property ForAZeroFactorElementGen_ItConsumesRandomnessForTheLength(object element, Iterations iterations)
        {
            Action action = () => TestWithSeed(seed =>
            {
                var elementGen = GC.Gen.Constant(element);
                var gen = GC.Gen.List(elementGen);

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations.Value, seed: seed);

                Assert.Equal(iterations.Value, sample.RandomnessConsumption);
            });

            return action.When(iterations.Value > 0);
        }

        [Property]
        public FsCheck.Property ForAOneFactorElementGen_ItConsumesRandomnessForTheLengthAndEachElement(Iterations iterations)
        {
            Action action = () => TestWithSeed(seed =>
            {
                var elementGen = GC.Gen.Int32();
                var gen = GC.Gen.List(elementGen);

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations.Value, seed: seed);

                var expectedRandomnessConsumption = sample.Values.Select(l => l.Count).Sum() + iterations.Value;
                Assert.Equal(expectedRandomnessConsumption, sample.RandomnessConsumption);
            });

            return action.When(iterations.Value > 0);
        }
    }
}
