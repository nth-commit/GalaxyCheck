using FluentAssertions;
using GalaxyCheck;
using System;
using System.Linq;
using Xunit;

namespace Tests.V2.GenTests.AutoGenTests
{
    public class AboutGeneratingClasses
    {
        private class EmptyClass { }

        [Fact]
        public void ItGeneratesAnEmptyClass()
        {
            var gen = Gen.Auto<EmptyClass>();

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
            var gen = Gen
                .AutoFactory()
                .RegisterType(Gen.Int32().Where(x => x != 0))
                .Create<ClassWithOneProperty>();

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
                .AutoFactory()
                .RegisterType(Gen.Int32().Where(x => x != 0))
                .Create<ClassWithTwoProperties>();

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
                .AutoFactory()
                .RegisterType(Gen.Int32().Where(x => x != 0))
                .Create<ClassWithOneNestedProperty>();

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.Property.Property.Should().NotBe(0);
        }

        private class ClassWithOneRecursiveProperty
        {
            public ClassWithOneRecursiveProperty Property { get; set; } = null!;
        }

        [Fact]
        public void ItErrorsWhenGeneratingAClassWithOneRecursiveProperty()
        {
            var gen = Gen
                .AutoFactory()
                .RegisterType(Gen.Int32().Where(x => x != 0))
                .Create<ClassWithOneRecursiveProperty>();

            Action action = () => gen.SampleOne(seed: 0);

            action.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator AutoGen: detected circular reference on type '*ClassWithOneRecursiveProperty' at path '$.Property'");
        }
    }
}
