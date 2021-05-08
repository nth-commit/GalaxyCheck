using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ByteGenTests
{
    public class AboutDefaults
    {
        [Property]
        public NebulaCheck.IGen<Test> TheDefaultMinimumIsTheMinimumByte() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<byte> SampleTraversal(GalaxyCheck.IGen<byte> gen) => gen.SampleOneTraversal(seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Byte();
                var gen1 = gen0.GreaterThanEqual(byte.MinValue);

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> TheDefaultMaximumIsTheMaximumByte() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<byte> SampleTraversal(GalaxyCheck.IGen<byte> gen) => gen.SampleOneTraversal(seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Byte();
                var gen1 = gen0.LessThanEqual(byte.MaxValue);

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> TheDefaultOriginIsTheMinimum() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            from min in Gen.Byte()
            select Property.ForThese(() =>
            {
                List<byte> SampleTraversal(GalaxyCheck.IGen<byte> gen) => gen.SampleOneTraversal(seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Byte().GreaterThanEqual(min);
                var gen1 = gen0.ShrinkTowards(min);

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
                List<byte> SampleTraversal(GalaxyCheck.IGen<byte> gen) => gen.SampleOneTraversal(seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.Byte();
                var gen1 = gen0.WithBias(GalaxyCheck.Gen.Bias.WithSize);

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });
    }
}
