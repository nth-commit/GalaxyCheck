using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.Int16GenTests
{
    public class AboutDefaults
    {
        [Property]
        public NebulaCheck.IGen<Test> TheDefaultMinimumIsTheMinimumInt16() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<short> SampleTraversal(GalaxyCheck.IGen<short> gen) => gen.SampleOneTraversal(seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Int16();
                var gen1 = gen0.GreaterThanEqual(short.MinValue);

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> TheDefaultMaximumIsTheMaximumInt16() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<short> SampleTraversal(GalaxyCheck.IGen<short> gen) => gen.SampleOneTraversal(seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Int16();
                var gen1 = gen0.LessThanEqual(short.MaxValue);

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> IfTheRangeSpansZero_TheDefaultOriginIsZero() =>
            from min in Gen.Int16().LessThanEqual(0)
            from max in Gen.Int16().GreaterThanEqual(0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<short> SampleTraversal(GalaxyCheck.IGen<short> gen) => gen.SampleOneTraversal(seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Int16().GreaterThanEqual(min).LessThanEqual(max);
                var gen1 = gen0.ShrinkTowards(0);

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> IfTheRangeIsPositive_TheDefaultOriginIsTheMinimum_BecauseItIsClosestToZero() =>
            from min in Gen.Int16().GreaterThanEqual(0)
            from max in Gen.Int16().GreaterThanEqual(min)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<short> SampleTraversal(GalaxyCheck.IGen<short> gen) => gen.SampleOneTraversal(seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Int16().GreaterThanEqual(min).LessThanEqual(max);
                var gen1 = gen0.ShrinkTowards(min);

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> IfTheRangeIsNegative_TheDefaultOriginIsTheMaximum_BecauseItIsClosestToZero() =>
            from min in Gen.Int16().LessThanEqual(0)
            from max in Gen.Int16().Between(min, 0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<short> SampleTraversal(GalaxyCheck.IGen<short> gen) => gen.SampleOneTraversal(seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Int16().GreaterThanEqual(min).LessThanEqual(max);
                var gen1 = gen0.ShrinkTowards(max);

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> TheDefaultBiasIsWithSize() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<short> SampleTraversal(GalaxyCheck.IGen<short> gen) => gen.SampleOneTraversal(seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Int16();
                var gen1 = gen0.WithBias(GalaxyCheck.Gen.Bias.WithSize);

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });
    }
}
