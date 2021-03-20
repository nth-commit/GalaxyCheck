using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.Int32GenTests
{
    public class AboutDefaults
    {
        [Property]
        public NebulaCheck.IGen<Test> TheDefaultMinimumIsTheMinimumInt32() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) => AboutDefaults.SampleTraversal(gen, seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Int32();
                var gen1 = gen0.GreaterThanEqual(int.MinValue);

                SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
            });

        [Property]
        public NebulaCheck.IGen<Test> TheDefaultMaximumIsTheMaximumInt32() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) => AboutDefaults.SampleTraversal(gen, seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Int32();
                var gen1 = gen0.LessThanEqual(int.MaxValue);

                SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
            });

        [Property]
        public NebulaCheck.IGen<Test> IfTheRangeSpansZero_TheDefaultOriginIsZero() =>
            from min in Gen.Int32().LessThanEqual(0)
            from max in Gen.Int32().GreaterThanEqual(0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) => AboutDefaults.SampleTraversal(gen, seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Int32().GreaterThanEqual(min).LessThanEqual(max);
                var gen1 = gen0.ShrinkTowards(0);

                SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
            });

        [Property]
        public NebulaCheck.IGen<Test> IfTheRangeIsPositive_TheDefaultOriginIsTheMinimum_BecauseItIsClosestToZero() =>
            from min in Gen.Int32().GreaterThanEqual(0)
            from max in Gen.Int32().GreaterThanEqual(min)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) => AboutDefaults.SampleTraversal(gen, seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Int32().GreaterThanEqual(min).LessThanEqual(max);
                var gen1 = gen0.ShrinkTowards(min);

                SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
            });

        [Property]
        public NebulaCheck.IGen<Test> IfTheRangeIsNegative_TheDefaultOriginIsTheMaximum_BecauseItIsClosestToZero() =>
            from min in Gen.Int32().LessThanEqual(0)
            from max in Gen.Int32().Between(min, 0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) => AboutDefaults.SampleTraversal(gen, seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Int32().GreaterThanEqual(min).LessThanEqual(max);
                var gen1 = gen0.ShrinkTowards(max);

                SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
            });

        [Property]
        public NebulaCheck.IGen<Test> TheDefaultBiasIsWithSize() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) => AboutDefaults.SampleTraversal(gen, seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Int32();
                var gen1 = gen0.WithBias(GalaxyCheck.Gen.Bias.WithSize);

                SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
            });

        private static List<int> SampleTraversal(GalaxyCheck.IGen<int> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
