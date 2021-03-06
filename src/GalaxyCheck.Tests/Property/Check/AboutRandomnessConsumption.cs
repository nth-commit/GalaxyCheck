﻿using FsCheck;
using FsCheck.Xunit;
using System;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;
using System.Linq;

namespace Tests.Property.Check
{
    [Properties(Arbitrary = new [] { typeof(ArbitraryIterations) })]
    public class AboutRandomnessConsumption
    {
        [Property(Arbitrary = new[] { typeof(ArbitraryGen) }, EndSize = 50)]
        public FsCheck.Property ItConsumesRandomnessLikeSample(Iterations iterations, int randomnessConsumptionPerIteration, object value)
        {
            var gen = GC.Gen.Create(parameters =>
            {
                var nextRng = Enumerable
                    .Range(0, randomnessConsumptionPerIteration)
                    .Aggregate(parameters.Rng, (rng, _) => rng.Next());

                return (value, GC.Gens.Parameters.GenParameters.Create(nextRng, parameters.Size));
            });

            Action test = () => TestWithSeed(seed =>
            {
                var property = gen.ForAll(_ => true);

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations.Value, seed: seed);
                var check = property.Check(iterations: iterations.Value, seed: seed);

                Assert.Equal(sample.RandomnessConsumption, check.RandomnessConsumption);
            });

            return test.When(iterations.Value > 0 && randomnessConsumptionPerIteration >= 0);
        }
    }
}
