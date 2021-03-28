using FluentAssertions;
using GalaxyCheck;
using System.Linq;
using Xunit;

namespace Tests.V2.GenTests.AutoGenTests
{
    public class AboutGeneratingRecords
    {
        private record EmptyRecord();

        [Fact]
        public void ItGeneratesAnEmptyRecord()
        {
            var gen = Gen.Auto<EmptyRecord>();

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
        }

        private record RecordWithOneProperty(int Property);

        [Fact]
        public void ItGeneratesARecordWithOneProperty()
        {
            var gen = Gen
                .Auto<RecordWithOneProperty>()
                .RegisterType(Gen.Int32().Where(x => x != 0));

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.Property.Should().NotBe(0);
        }

        private record RecordWithTwoProperties(int Property1, int Property2);

        [Fact]
        public void ItGeneratesARecordWithTwoProperties()
        {
            var gen = Gen
                .Auto<RecordWithTwoProperties>()
                .RegisterType(Gen.Int32().Where(x => x != 0));

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.Property1.Should().NotBe(0);
            instance.Property2.Should().NotBe(0);
        }

        private record RecordWithOneNestedProperty(RecordWithOneProperty Property);

        [Fact]
        public void ItGeneratesARecordWithOneNestedProperty()
        {
            var gen = Gen
                .Auto<RecordWithOneNestedProperty>()
                .RegisterType(Gen.Int32().Where(x => x != 0));

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.Property.Property.Should().NotBe(0);
        }
    }
}
