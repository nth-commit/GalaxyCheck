using FluentAssertions;
using GalaxyCheck;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutStability
    {
        private class ClassWithPropertiesV1
        {
            public int Property { get; set; }
        }

        private class ClassWithPropertiesV2
        {
            public int A_Property { get; set; }
            public int Property { get; set; }
            public int Z_Property { get; set; }
        }

        [NebulaCheck.Property]
        public void AddingNewPropertiesDoesNotChangeHowExistingPropertiesAreGenerated([Seed] int seed, [Size] int size)
        {
            var gen0 = Gen.Create<ClassWithPropertiesV1>().Advanced.SetRngWaypoint();
            var sample0 = gen0.SampleOne(seed: seed, size: size);

            var gen1 = Gen.Create<ClassWithPropertiesV2>().Advanced.SetRngWaypoint();
            var sample1 = gen1.SampleOne(seed: seed, size: size);

            sample1.Property.Should().Be(sample0.Property);
        }

        private class ClassWithFieldsV1
        {
            public int Field = 0;
        }

        private class ClassWithFieldsV2
        {
            public int A_Field = 0;
            public int Field = 0;
            public int Z_Field = 0;
        }

        [NebulaCheck.Property]
        public void AddingNewFieldsDoesNotChangeHowExistingFieldsAreGenerated([Seed] int seed, [Size] int size)
        {
            var gen0 = Gen.Create<ClassWithFieldsV1>().Advanced.SetRngWaypoint();
            var sample0 = gen0.SampleOne(seed: seed, size: size);

            var gen1 = Gen.Create<ClassWithFieldsV2>().Advanced.SetRngWaypoint();
            var sample1 = gen1.SampleOne(seed: seed, size: size);

            sample1.Field.Should().Be(sample0.Field);
        }

        private record RecordV1(int Property);

        private record RecordV2(
            int A_Property,
            int Property,
            int Z_Property);

        [NebulaCheck.Property]
        public void AddingNewPropertiesToStandardRecordConstructorDoesNotChangeHowExistingPropertiesAreGenerated([Seed] int seed, [Size] int size)
        {
            var gen0 = Gen.Create<RecordV1>().Advanced.SetRngWaypoint();
            var sample0 = gen0.SampleOne(seed: seed, size: size);

            var gen1 = Gen.Create<RecordV2>().Advanced.SetRngWaypoint();
            var sample1 = gen1.SampleOne(seed: seed, size: size);

            sample1.Property.Should().Be(sample0.Property);
        }
    }
}
