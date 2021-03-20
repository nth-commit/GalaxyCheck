using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.Int32GenTests
{
    public class AboutRandomnessConsumption
    {
        [Property]
        public NebulaCheck.IGen<Test> IfSizeIsBelowChaosSize_ItConsumesRandomnessOncePerIteration() =>
            from iterations in DomainGen.Iterations()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size(allowChaos: false)
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int32();

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations, seed: seed, size: size);

                sample.RandomnessConsumption.Should().Be(sample.Values.Count);
            });
    }
}
