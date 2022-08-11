using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutNullableGeneration
    {
        private record RecordWithNullableStruct(int? Property);

        [Property]
        public void ItGeneratesNullableStructsInStandardConstructors([Seed] int seed, [Size] int size)
        {
            List<int?> SampleTraversal(GalaxyCheck.IGen<int?> gen) =>
                AboutNullableGeneration.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.NullableStruct(GalaxyCheck.Gen.Int32());
            var gen1 = GalaxyCheck.Gen.Create<RecordWithNullableStruct>().Select(r => r.Property);

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample1.Should().BeEquivalentTo(sample0);
        }

        private record RecordWithNullableReference(string? Property);

        [Property]
        public void ItGeneratesNullableReferencesInStandardConstructors([Seed] int seed, [Size] int size)
        {
            List<string?> SampleTraversal(GalaxyCheck.IGen<string?> gen) =>
                AboutNullableGeneration.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.Nullable(GalaxyCheck.Gen.String());
            var gen1 = GalaxyCheck.Gen.Create<RecordWithNullableReference>().Select(r => r.Property);

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample1.Should().BeEquivalentTo(sample0);
        }

        private class ClassWithNullableStructProperty
        {
            public int? Property { get; set; }
        }

        [Property]
        public void ItGeneratesNullableStructsInPropertiesOfDefaultConstructors([Seed] int seed, [Size] int size)
        {
            List<int?> SampleTraversal(GalaxyCheck.IGen<int?> gen) =>
                AboutNullableGeneration.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.NullableStruct(GalaxyCheck.Gen.Int32());
            var gen1 = GalaxyCheck.Gen.Create<ClassWithNullableStructProperty>().Select(r => r.Property);

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample1.Should().BeEquivalentTo(sample0);
        }

        private class ClassWithNullableReferenceProperty
        {
            public string? Property { get; set; }
        }

        [Property]
        public void ItGeneratesNullableReferencesInPropertiesOfDefaultConstructors([Seed] int seed, [Size] int size)
        {
            List<string?> SampleTraversal(GalaxyCheck.IGen<string?> gen) =>
                AboutNullableGeneration.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.Nullable(GalaxyCheck.Gen.String());
            var gen1 = GalaxyCheck.Gen.Create<ClassWithNullableReferenceProperty>().Select(r => r.Property);

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample1.Should().BeEquivalentTo(sample0);
        }

        private class ClassWithNullableStructField
        {
            public int? Property;

            public ClassWithNullableStructField()
            {
                Property = null;
            }
        }

        [Property]
        public void ItGeneratesNullableStructsInFieldsOfDefaultConstructors([Seed] int seed, [Size] int size)
        {
            List<int?> SampleTraversal(GalaxyCheck.IGen<int?> gen) =>
                AboutNullableGeneration.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.NullableStruct(GalaxyCheck.Gen.Int32());
            var gen1 = GalaxyCheck.Gen.Create<ClassWithNullableStructField>().Select(r => r.Property);

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample1.Should().BeEquivalentTo(sample0);
        }

        private class ClassWithNullableReferenceField
        {
            public string? Property;

            public ClassWithNullableReferenceField()
            {
                Property = null;
            }
        }

        [Property]
        public void ItGeneratesNullableReferencesInFieldsOfDefaultConstructors([Seed] int seed, [Size] int size)
        {
            List<string?> SampleTraversal(GalaxyCheck.IGen<string?> gen) =>
                AboutNullableGeneration.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.Nullable(GalaxyCheck.Gen.String());
            var gen1 = GalaxyCheck.Gen.Create<ClassWithNullableReferenceField>().Select(r => r.Property);

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample1.Should().BeEquivalentTo(sample0);
        }

        [Property]
        public void ItGeneratesNullablesStructsInCollections([Seed] int seed, [Size] int size)
        {
            List<IReadOnlyList<int?>> SampleTraversal(GalaxyCheck.IGen<IReadOnlyList<int?>> gen) =>
                AboutNullableGeneration.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.NullableStruct(GalaxyCheck.Gen.Int32()).ListOf();
            var gen1 = GalaxyCheck.Gen.Create<IReadOnlyList<int?>>();

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample1.Should().BeEquivalentTo(sample0);
        }

        private record RecordWithNullableReferenceInCollection(IReadOnlyList<string?> Property);


        [Property]
        public void ItGeneratesNullablesReferencesInCollections([Seed] int seed, [Size] int size)
        {
            List<IReadOnlyList<string?>> SampleTraversal(GalaxyCheck.IGen<IReadOnlyList<string?>> gen) =>
                AboutNullableGeneration.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.Nullable(GalaxyCheck.Gen.String()).ListOf();
            var gen1 = GalaxyCheck.Gen.Create<RecordWithNullableReferenceInCollection>().Select(x => x.Property);

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample1.Should().BeEquivalentTo(sample0);
        }

        [Property]
        public void ItGeneratesNullablesStructsInSets([Seed] int seed, [Size] int size)
        {
            List<IReadOnlySet<int?>> SampleTraversal(GalaxyCheck.IGen<IReadOnlySet<int?>> gen) =>
                AboutNullableGeneration.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.NullableStruct(GalaxyCheck.Gen.Int32()).SetOf();
            var gen1 = GalaxyCheck.Gen.Create<IReadOnlySet<int?>>();

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample1.Should().BeEquivalentTo(sample0);
        }

        private record RecordWithNullableReferenceInSet(IReadOnlySet<string?> Property);


        [Property]
        public void ItGeneratesNullablesReferencesInSets([Seed] int seed, [Size] int size)
        {
            List<IReadOnlySet<string?>> SampleTraversal(GalaxyCheck.IGen<IReadOnlySet<string?>> gen) =>
                AboutNullableGeneration.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.Nullable(GalaxyCheck.Gen.String()).SetOf();
            var gen1 = GalaxyCheck.Gen.Create<RecordWithNullableReferenceInSet>().Select(x => x.Property);

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample1.Should().BeEquivalentTo(sample0);
        }

        private static List<T> SampleTraversal<T>(GalaxyCheck.IGen<T> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
