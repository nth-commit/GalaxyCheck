using FluentAssertions;
using GalaxyCheck;
using System;
using System.Linq;
using Xunit;

namespace Tests.V2.GenTests.CreateGenTests
{
    public class AboutGeneratingClasses
    {
        private class EmptyClass { }

        [Fact]
        public void ItGeneratesAnEmptyClass()
        {
            var gen = Gen.Create<EmptyClass>();

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
        }

        private class ClassWithOneProperty
        {
            public int Property { get; set; }
        }

        [Fact]
        public void ItGeneratesAClassWithOneProperty()
        {
            var gen = Gen.Create<ClassWithOneProperty>().RegisterType(Gen.Int32().Where(x => x != 0));

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.Property.Should().NotBe(0);
        }

        private class ClassWithTwoProperties
        {
            public int Property1 { get; set; }

            public int Property2 { get; set; }
        }

        [Fact]
        public void ItGeneratesAClassWithTwoProperties()
        {
            var gen = Gen
                .Create<ClassWithTwoProperties>()
                .RegisterType(Gen.Int32().Where(x => x != 0));

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.Property1.Should().NotBe(0);
            instance.Property2.Should().NotBe(0);
        }

        private class ClassWithOneNestedProperty
        {
            public ClassWithOneProperty Property { get; set; } = null!;
        }

        [Fact]
        public void ItGeneratesAClassWithOneNestedProperty()
        {
            var gen = Gen
                .Create<ClassWithOneNestedProperty>()
                .RegisterType(Gen.Int32().Where(x => x != 0));

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.Property.Property.Should().NotBe(0);
        }

        private class ClassWithRecursiveProperty
        {
            public ClassWithRecursiveProperty Property { get; set; } = null!;
        }

        [Fact]
        public void ItErrorsWhenGeneratingAClassWithOneNestedProperty()
        {
            var gen = Gen
                .Create<ClassWithRecursiveProperty>()
                .RegisterType(Gen.Int32().Where(x => x != 0));

            Action action = () => gen.SampleOne(seed: 0);

            action.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator CreateGen: detected circular reference on type '*ClassWithRecursiveProperty' at path '$.Property'");
        }
    }
}
