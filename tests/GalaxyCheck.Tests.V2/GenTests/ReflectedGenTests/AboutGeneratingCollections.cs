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
    public class AboutGeneratingCollections
    {
        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateLists() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<List<int>> SampleTraversal(GalaxyCheck.IGen<List<int>> gen) =>
                    AboutGeneratingCollections.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<List<int>>();
                var gen1 = GalaxyCheck.Gen.Int32().ListOf().Select(x => x.ToList());

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateIReadOnlyLists() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<IReadOnlyList<int>> SampleTraversal(GalaxyCheck.IGen<IReadOnlyList<int>> gen) =>
                    AboutGeneratingCollections.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<IReadOnlyList<int>>();
                var gen1 = GalaxyCheck.Gen.Int32().ListOf();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateImmutableLists() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<ImmutableList<int>> SampleTraversal(GalaxyCheck.IGen<ImmutableList<int>> gen) =>
                    AboutGeneratingCollections.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<ImmutableList<int>>();
                var gen1 = GalaxyCheck.Gen.Int32().ListOf().Select(x => x.ToImmutableList());

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateILists() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<IList<int>> SampleTraversal(GalaxyCheck.IGen<IList<int>> gen) =>
                    AboutGeneratingCollections.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<IList<int>>();
                var gen1 = GalaxyCheck.Gen.Int32().ListOf().Select(x => x.ToList());

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateIReadOnlyCollections() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<IReadOnlyCollection<int>> SampleTraversal(GalaxyCheck.IGen<IReadOnlyCollection<int>> gen) =>
                    AboutGeneratingCollections.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<IReadOnlyCollection<int>>();
                var gen1 = GalaxyCheck.Gen.Int32().ListOf();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateIEnumerables() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<IEnumerable<int>> SampleTraversal(GalaxyCheck.IGen<IEnumerable<int>> gen) =>
                    AboutGeneratingCollections.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<IEnumerable<int>>();
                var gen1 = GalaxyCheck.Gen.Int32().ListOf();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        private class ClassWithOneProperty
        {
            public int Property { get; set; }
        }

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateListsOfAnObjectWithDefaultConstructor() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<List<ClassWithOneProperty>> SampleTraversal(GalaxyCheck.IGen<List<ClassWithOneProperty>> gen) =>
                    AboutGeneratingCollections.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<List<ClassWithOneProperty>>();
                var gen1 = GalaxyCheck.Gen.Create<ClassWithOneProperty>().ListOf().Select(x => x.ToList());

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        private record RecordWithOneProperty(int Property);

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateListsOfAnObjectWithNonDefaultConstructor() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<List<RecordWithOneProperty>> SampleTraversal(GalaxyCheck.IGen<List<RecordWithOneProperty>> gen) =>
                    AboutGeneratingCollections.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<List<RecordWithOneProperty>>();
                var gen1 = GalaxyCheck.Gen.Create<RecordWithOneProperty>().ListOf().Select(x => x.ToList());

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        private static List<T> SampleTraversal<T>(GalaxyCheck.IGen<T> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
