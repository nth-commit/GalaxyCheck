using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.CastGenTests
{
    public class AboutCasting
    {
        private class ReferenceType { }

        [Property]
        public void ItCanUpcastReferenceTypes([Seed] int seed, [Size] int size)
        {
            List<object> SampleTraversal(GalaxyCheck.IGen<object> gen) =>
                gen.SampleOneTraversal(seed: seed, size: size);

            var reference = new ReferenceType();
            var gen0 = GalaxyCheck.Gen.Constant(reference).Cast<object>();
            var gen1 = GalaxyCheck.Gen.Constant<object>(reference);

            SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
        }

        [Property]
        public void ItCanDowncastReferenceTypes([Seed] int seed, [Size] int size)
        {
            List<object> SampleTraversal(GalaxyCheck.IGen<object> gen) =>
                gen.SampleOneTraversal(seed: seed, size: size);

            var reference = new ReferenceType();
            var gen0 = GalaxyCheck.Gen.Constant<object>(reference);
            var gen1 = GalaxyCheck.Gen.Constant(reference).Cast<object>();

            SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
        }

        [Property]
        public void ItCanUpcastValueTypes([Seed] int seed, [Size] int size)
        {
            List<object> SampleTraversal(GalaxyCheck.IGen<object> gen) =>
                gen.SampleOneTraversal(seed: seed, size: size);

            var gen0 = GalaxyCheck.Gen.Int32().Cast<object>();
            var gen1 = GalaxyCheck.Gen.Int32().Select(x => (object)x);

            SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
        }

        [Property]
        public void ItCanDowncastValueTypes([Seed] int seed, [Size] int size)
        {
            List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) =>
                gen.SampleOneTraversal(seed: seed, size: size);

            var gen0 = GalaxyCheck.Gen.Int32().Select(x => (object)x).Cast<int>();
            var gen1 = GalaxyCheck.Gen.Int32();

            SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
        }

        [Property]
        public void ItCanCastTypesThatHaveAnExplicitCast([Seed] int seed, [Size] int size)
        {
            List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) =>
                gen.SampleOneTraversal(seed: seed, size: size);

            var gen0 = GalaxyCheck.Gen.Int32().Select(x => (long)x).Cast<int>();
            var gen1 = GalaxyCheck.Gen.Int32();

            SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
        }
    }
}
