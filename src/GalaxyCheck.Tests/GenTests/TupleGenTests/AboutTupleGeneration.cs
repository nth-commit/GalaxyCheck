using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.TupleGenTests
{
    public class AboutTupleGeneration
    {
        [Property]
        public void TwoGeneratesLikeZip(
            [Seed] int seed,
            [Size] int size,
            [PickGen] GalaxyCheck.IGen<object> gen)
        {
            List<(object, object)> SampleTraversal(GalaxyCheck.IGen<(object, object)> gen) =>
                gen.SampleOneTraversal(seed: seed, size: size);

            var gen0 = gen.Two();
            var gen1 = GalaxyCheck.Gen.Zip(gen, gen);

            SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
        }

        [Property]
        public void ThreeGeneratesLikeZip(
            [Seed] int seed,
            [Size] int size,
            [PickGen] GalaxyCheck.IGen<object> gen)
        {
            List<(object, object, object)> SampleTraversal(GalaxyCheck.IGen<(object, object, object)> gen) =>
                gen.SampleOneTraversal(seed: seed, size: size);

            var gen0 = gen.Three();
            var gen1 = GalaxyCheck.Gen.Zip(gen, gen, gen);

            SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
        }

        [Property]
        public void FourGeneratesLikeZip(
            [Seed] int seed,
            [Size] int size,
            [PickGen] GalaxyCheck.IGen<object> gen)
        {
            List<(object, object, object, object)> SampleTraversal(GalaxyCheck.IGen<(object, object, object, object)> gen) =>
                gen.SampleOneTraversal(seed: seed, size: size);

            var gen0 = gen.Four();
            var gen1 = GalaxyCheck.Gen.Zip(gen, gen, gen, gen);

            SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
        }
    }
}
