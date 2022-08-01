using FluentAssertions;
using Xunit;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutSpecialBehaviorsInTheFactory
    {
        private record RecordProperty(int Value);

        private record Record(RecordProperty PropertyA, RecordProperty PropertyB);

        [Fact]
        public void TypeRegistrationsCanReferenceTheFactoryItself()
        {
            var propertyAValue = new RecordProperty(1);
            var propertyBValue = new RecordProperty(2);

            var genFactory = GalaxyCheck.Gen
                .Factory()
                .RegisterType(GalaxyCheck.Gen.Constant(propertyAValue))
                .RegisterType((factory) => factory
                    .Create<Record>()
                    .OverrideMember(x => x.PropertyB, GalaxyCheck.Gen.Constant(propertyBValue)));

            var gen = genFactory.Create<Record>();
            var sample = GalaxyCheck.Extensions.SampleOne(gen);

            sample.Should().Be(new Record(propertyAValue, propertyBValue));
        }

        [Fact]
        public void TypeRegistrationsReferencingTheFactoryOnlyUsePriorRegistrations()
        {
            var propertyAValue = new RecordProperty(1);
            var propertyBValue = new RecordProperty(2);
            var defaultPropertyValue = new RecordProperty(3);

            var genFactory = GalaxyCheck.Gen
                .Factory()
                .RegisterType(GalaxyCheck.Gen.Constant(propertyAValue.Value)) // Register as an int to control the generation somewhat (but not explicitly)
                .RegisterType((factory) => factory
                    .Create<Record>()
                    .OverrideMember(x => x.PropertyB, GalaxyCheck.Gen.Constant(propertyBValue)))
                .RegisterType(GalaxyCheck.Gen.Constant(defaultPropertyValue));

            var gen = genFactory.Create<Record>();
            var sample = GalaxyCheck.Extensions.SampleOne(gen);

            sample.Should().Be(new Record(propertyAValue, propertyBValue));
        }
    }
}
