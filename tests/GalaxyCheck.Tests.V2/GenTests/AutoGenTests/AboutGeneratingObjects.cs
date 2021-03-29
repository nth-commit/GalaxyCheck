using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using Xunit;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.AutoGenTests
{
    public class AboutGeneratingObjects
    {
        private class EmptyClass { }

        [Fact]
        public void ItCanGenerateAnEmptyObject()
        {
            var gen = GalaxyCheck.Gen.Auto<EmptyClass>();

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
        }

        private class ClassWithOneProperty
        {
            public int Property { get; set; }
        }

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateAnObjectWithAProperty() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .AutoFactory()
                    .RegisterType(GalaxyCheck.Gen.Int32().Where(x => x != 0))
                    .Create<ClassWithOneProperty>();

                var instance = gen.SampleOne(seed: seed, size: size);

                instance.Should().NotBeNull();
                instance.Property.Should().NotBe(0);
            });

        private class ClassWithTwoProperties
        {
            public int Property1 { get; set; }

            public int Property2 { get; set; }
        }

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateAnObjectWithTwoProperties() =>
            from value1 in DomainGen.Any()
            from value2 in DomainGen.Any()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .AutoFactory()
                    .RegisterType(GalaxyCheck.Gen.Int32().Where(x => x != 0))
                    .Create<ClassWithTwoProperties>();

                var instance = gen.SampleOne(seed: seed, size: size);

                instance.Should().NotBeNull();
                instance.Property1.Should().NotBe(0);
                instance.Property2.Should().NotBe(0);
            });

        private class ClassWithOneField
        {
#pragma warning disable CS0649
            public int Field;
#pragma warning restore CS0649
        }

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateAnObjectWithAField() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .AutoFactory()
                    .RegisterType(GalaxyCheck.Gen.Int32().Where(x => x != 0))
                    .Create<ClassWithOneField>();

                var instance = gen.SampleOne(seed: seed, size: size);

                instance.Should().NotBeNull();
                instance.Field.Should().NotBe(0);
            });

        private class ClassWithOneReadOnlyProperty
        {
            public int Property { get; } = 0;
        }

        [Fact]
        public void ItAvoidsGenerationOfReadOnlyProperties()
        {
            var gen = GalaxyCheck.Gen.Auto<ClassWithOneReadOnlyProperty>();

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
        }

        private class ClassWithOnePropertyWithPrivateSetter
        {
            public int Property { get; private set; } = 0;
        }

        [Fact]
        public void ItAvoidsGenerationOfPropertiesWithPrivateSetter()
        {
            var gen = GalaxyCheck.Gen.Auto<ClassWithOnePropertyWithPrivateSetter>();

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
        }
    }
}
