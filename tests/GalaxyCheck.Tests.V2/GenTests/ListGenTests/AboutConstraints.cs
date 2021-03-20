using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ListGenTests
{
    public class AboutConstraints
    {
        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesWithCountGreaterThanOrEqualMinimumCount() =>
            from elementGen in DomainGen.Gen()
            from minCount in Gen.Int32().Between(0, 10)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = elementGen.ListOf().WithCountGreaterThanEqual(minCount);

                var exampleSpace = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size);

                exampleSpace.Traverse().Take(100).Should().OnlyContain(value => value.Count >= minCount);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesWithCountLessThanOrEqualMaximumCount() =>
            from elementGen in DomainGen.Gen()
            from maxCount in Gen.Int32().Between(0, 10)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = elementGen.ListOf().WithCountLessThanEqual(maxCount);

                var exampleSpace = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size);

                exampleSpace.Traverse().Take(100).Should().OnlyContain(value => value.Count <= maxCount);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesWithOfTheSpecificCount() =>
            from elementGen in DomainGen.Gen()
            from count in Gen.Int32().Between(0, 20)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = elementGen.ListOf().OfCount(count);

                var exampleSpace = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size);

                exampleSpace.Traverse().Take(100).Should().OnlyContain(value => value.Count == count);
            });
    }
}
