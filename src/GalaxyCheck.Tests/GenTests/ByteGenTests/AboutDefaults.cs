using FluentAssertions;
using NebulaCheck;
using System.Collections.Generic;
using static Tests.V2.DomainGenAttributes;
using Property = NebulaCheck.Property;

namespace Tests.V2.GenTests.ByteGenTests
{
    public class AboutDefaults
    {
        [Property]
        public void TheDefaultMinimumIsTheMinimumByte([Seed] int seed, [Size] int size)
        {
            List<byte> SampleTraversal(GalaxyCheck.IGen<byte> gen) => gen.SampleOneTraversal(seed: seed, size: size);

            var gen0 = GalaxyCheck.Gen.Byte();
            var gen1 = gen0.GreaterThanEqual(byte.MinValue);

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample0.Should().BeEquivalentTo(sample1);
        }

        [Property]
        public void TheDefaultMaximumIsTheMaximumByte([Seed] int seed, [Size] int size)
        {
            List<byte> SampleTraversal(GalaxyCheck.IGen<byte> gen) => gen.SampleOneTraversal(seed: seed, size: size);

            var gen0 = GalaxyCheck.Gen.Byte();
            var gen1 = gen0.LessThanEqual(byte.MaxValue);

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample0.Should().BeEquivalentTo(sample1);
        }

        [Property]
        public void TheDefaultOriginIsTheMinimum([Seed] int seed, [Size] int size, byte min)
        {
            List<byte> SampleTraversal(GalaxyCheck.IGen<byte> gen) => gen.SampleOneTraversal(seed: seed, size: size);

            var gen0 = GalaxyCheck.Gen.Byte().GreaterThanEqual(min);
            var gen1 = gen0.ShrinkTowards(min);

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample0.Should().BeEquivalentTo(sample1);
        }

        [Property]
        public void TheDefaultBiasIsWithSize([Seed] int seed, [Size] int size)
        {
            List<byte> SampleTraversal(GalaxyCheck.IGen<byte> gen) => gen.SampleOneTraversal(seed: seed, size: size);

            var gen0 = GalaxyCheck.Gen.Byte();
            var gen1 = gen0.WithBias(GalaxyCheck.Gen.Bias.WithSize);

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample0.Should().BeEquivalentTo(sample1);
        }
    }
}
