using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.Int64GenTests
{
    public class AboutRandomnessConsumption
    {
        [Property]
        public NebulaCheck.IGen<Test> IfSizeIsBelowChaosSize_AndRangeIsNonZero_ItConsumesRandomnessOncePerIteration() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size(allowChaos: false)
            from iterations in DomainGen.Iterations()
            from x in Gen.Int64()
            from y in Gen.Int64()
            where x != y
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int64().Between(x, y);

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations, seed: seed, size: size);

                sample.RandomnessConsumption.Should().Be(sample.Values.Count);
            });

        [Property]
        public NebulaCheck.IGen<Test> IfSizeIsBelowChaosSize_AndRangeIsZero_ItDoesNotConsumeRandomness() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size(allowChaos: false)
            from iterations in DomainGen.Iterations()
            from x in Gen.Int64()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int64().Between(x, x);

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations, seed: seed, size: size);

                sample.RandomnessConsumption.Should().Be(0);
            });
    }
}
