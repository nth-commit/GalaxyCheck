using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutGeneratingSets
    {
        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateISets() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<ISet<int>> SampleTraversal(GalaxyCheck.IGen<ISet<int>> gen) =>
                    AboutGeneratingSets.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<ISet<int>>();
                var gen1 = GalaxyCheck.Gen.Int32().SetOf().Select(x => x.ToHashSet());

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateHashSets() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<HashSet<int>> SampleTraversal(GalaxyCheck.IGen<HashSet<int>> gen) =>
                    AboutGeneratingSets.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<HashSet<int>>();
                var gen1 = GalaxyCheck.Gen.Int32().SetOf().Select(x => x.ToHashSet());

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateImmutableHashSets() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<ImmutableHashSet<int>> SampleTraversal(GalaxyCheck.IGen<ImmutableHashSet<int>> gen) =>
                    AboutGeneratingSets.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<ImmutableHashSet<int>>();
                var gen1 = GalaxyCheck.Gen.Int32().SetOf().Select(x => x.ToImmutableHashSet());

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        private static List<T> SampleTraversal<T>(GalaxyCheck.IGen<T> gen, int seed, int size) => gen.SampleOneTraversal(seed: seed, size: size);
    }
}
