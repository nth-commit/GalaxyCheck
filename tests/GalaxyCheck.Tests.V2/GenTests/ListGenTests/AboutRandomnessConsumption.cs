using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ListGenTests
{
    public class AboutRandomnessConsumption
    {
        [Property]
        public NebulaCheck.IGen<Test> IfSizeIsBelowChaosSize_AndCountIsOfARange_ItConsumesRandomnessForTheCountAndForEachElement() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size(allowChaos: false)
            from elementGen in DomainGen.Gen()
            select Property.ForThese(() =>
            {
                var gen = elementGen.ListOf().WithCountBetween(0, 20);

                var listSample = gen.Advanced.SampleOneWithMetrics(seed: seed, size: size);
                var elementSample = elementGen.Advanced.SampleWithMetrics(iterations: listSample.Value.Count, seed: seed, size: size);

                listSample.RandomnessConsumption.Should().Be(elementSample.RandomnessConsumption + 1);
            });

        [Property]
        public NebulaCheck.IGen<Test> IfSizeIsBelowChaosSize_AndCountIsSpecific_ItConsumesRandomnessForEachElement() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size(allowChaos: false)
            from elementGen in DomainGen.Gen()
            from count in Gen.Int32().Between(0, 20)
            select Property.ForThese(() =>
            {
                var gen = elementGen.ListOf().OfCount(count);

                var listSample = gen.Advanced.SampleOneWithMetrics(seed: seed, size: size);
                var elementSample = elementGen.Advanced.SampleWithMetrics(iterations: listSample.Value.Count, seed: seed, size: size);

                listSample.RandomnessConsumption.Should().Be(elementSample.RandomnessConsumption);
            });
    }
}
