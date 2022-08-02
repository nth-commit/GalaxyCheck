using FluentAssertions;
using GalaxyCheck;
using System;
using System.Linq;
using Xunit;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutConstructorSelection
    {
        private class ImplicitDefaultConstructor
        {
            public int Property { get; set; }
        }

        [Fact]
        public void WhenThereIsAnImplicitDefaultConstructor_ItUsesTheDefaultConstructor()
        {
            var gen = Gen
                .Factory()
                .RegisterType(Gen.Int32().Where(x => x != 0))
                .Create<ImplicitDefaultConstructor>();

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.Property.Should().NotBe(0);
        }

        private class ExplicitDefaultConstructor
        {
            public int Property { get; set; }

            public ExplicitDefaultConstructor()
            {
            }
        }

        [Fact]
        public void WhenThereIsAnExplicitDefaultConstructor_ItUsesTheDefaultConstructor()
        {
            var gen = Gen
                .Factory()
                .RegisterType(Gen.Int32().Where(x => x != 0))
                .Create<ExplicitDefaultConstructor>();

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.Property.Should().NotBe(0);
        }

        private class ConstructorWithOneArgument
        {
            public int? Property { get; set; }

            public ConstructorWithOneArgument(int property)
            {
                Property = property;
            }
        }

        [Fact]
        public void WhenThereIsAConstructorWithOneArgument_ItUsesTheDefaultConstructor()
        {
            var gen = Gen.Create<ConstructorWithOneArgument>();

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.Property.Should().NotBeNull();
        }

        private class DefaultConstructorAndAnotherWithOneArgument
        {
            public bool WasCreatedWithDefaultConstructor { get; }

            public DefaultConstructorAndAnotherWithOneArgument()
            {
                WasCreatedWithDefaultConstructor = true;
            }

            public DefaultConstructorAndAnotherWithOneArgument(int parameter)
            {
                WasCreatedWithDefaultConstructor = false;
            }
        }

        [Fact]
        public void WhenThereIsADefaultConstructorAndAnotherWithOneArgument_ItUsesTheDefaultConstructor()
        {
            var gen = Gen.Create<DefaultConstructorAndAnotherWithOneArgument>();

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.WasCreatedWithDefaultConstructor.Should().BeTrue();
        }

        private class ConstructorWithOneArgumentAndAnotherWithTwoArguments
        {
            public int? Property1 { get; set; }

            public int? Property2 { get; set; }

            public ConstructorWithOneArgumentAndAnotherWithTwoArguments(int property1)
            {
                Property1 = property1;
            }

            public ConstructorWithOneArgumentAndAnotherWithTwoArguments(int property1, int property2)
            {
                Property1 = property1;
                Property2 = property2;
            }
        }

        [Fact]
        public void WhenThereIsAConstructorWithOneArgumentAndAnotherWithTwoArguments_ItUsesTheConstructorWithTwoArguments()
        {
            var gen = Gen.Create<ConstructorWithOneArgumentAndAnotherWithTwoArguments>();

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.Property1.Should().NotBeNull();
            instance.Property2.Should().NotBeNull();
        }

        private class ClassWithNoPublicConstructor
        {
            private ClassWithNoPublicConstructor()
            {
            }
        }

        [Fact]
        public void ItErrorsWhenThereIsNoPublicConstructor()
        {
            var gen = Gen.Create<ClassWithNoPublicConstructor>();

            Action action = () => gen.SampleOne(seed: 0);

            action.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator ReflectedGen: could not resolve type '*ClassWithNoPublicConstructor'");
        }
    }
}
