using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Test = NebulaCheck.Test;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;


namespace Tests.V2.GenTests.SetGenTests
{
    public class AboutConstraints
    {
        [Property]
        public NebulaCheck.IGen<Test> ItProducesDistinctValuesWithCountGreaterThanOrEqualMinimumCount() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            from minCount in Gen.Int32().Between(0, 10)
            select Property.ForThese(() =>
            {
                var elementGen = GalaxyCheck.Gen.Int32();
                var gen = elementGen.SetOf().WithCountGreaterThanEqual(minCount);

                var sample = gen.SampleOneTraversal(seed: seed, size: size);

                sample.Should().OnlyContain(set => set.Count >= minCount && set.Distinct().SequenceEqual(set));
            });

        [Property]
        public NebulaCheck.IGen<Test> ItProducesDistinctValuesWithCountLessThanOrEqualMaximumCount() =>
            from maxCount in Gen.Int32().Between(0, 10)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var elementGen = GalaxyCheck.Gen.Int32();
                var gen = elementGen.SetOf().WithCountLessThanEqual(maxCount);

                var sample = gen.SampleOneTraversal(seed: seed, size: size);

                sample.Should().OnlyContain(set => set.Count <= maxCount && set.Distinct().SequenceEqual(set));
            });

        [Property]
        public NebulaCheck.IGen<Test> ItProducesDistinctValuesWithOfTheSpecificCount() =>
            from count in Gen.Int32().Between(0, 20)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var elementGen = GalaxyCheck.Gen.Int32();
                var gen = elementGen.SetOf().WithCount(count);

                var sample = gen.SampleOneTraversal(seed: seed, size: size);

                sample.Should().OnlyContain(set => set.Count == count && set.Distinct().SequenceEqual(set));
            });
    }
}
