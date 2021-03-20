using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.InfiniteGenTests
{
    public class AboutRandomnessConsumption
    {
        [Property]
        public NebulaCheck.IGen<Test> ItConsumesRandomnessOncePerIteration() =>
            from elementGen in DomainGen.Gen()
            from iterations in DomainGen.Iterations()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = elementGen.InfiniteOf();

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations, seed: seed, size: size);

                sample.RandomnessConsumption.Should().Be(iterations);
            });
    }
}
