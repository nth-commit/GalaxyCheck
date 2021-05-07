using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutGeneratingRecords
    {
        private record EmptyRecord();

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateAnEmptyRecord() =>
            from value in DomainGen.Any()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Create<EmptyRecord>();

                var instance = gen.SampleOne(seed: seed, size: size);

                instance.Should().NotBeNull();
            });

        private record RecordWithOneProperty(int Property);

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateARecordWithOneProperty() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .Factory()
                    .RegisterType(GalaxyCheck.Gen.Int32().Where(x => x != 0))
                    .Create<RecordWithOneProperty>();

                var instance = gen.SampleOne(seed: seed, size: size);

                instance.Should().NotBeNull();
                instance.Property.Should().NotBe(0);
            });

        private record RecordWithTwoProperties(int Property1, int Property2);

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateARecordWithTwoProperties() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .Factory()
                    .RegisterType(GalaxyCheck.Gen.Int32().Where(x => x != 0))
                    .Create<RecordWithTwoProperties>();

                var instance = gen.SampleOne(seed: 0);

                instance.Should().NotBeNull();
                instance.Property1.Should().NotBe(0);
                instance.Property2.Should().NotBe(0);
            });
    }
}
