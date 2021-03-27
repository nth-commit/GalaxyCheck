using FluentAssertions;
using GalaxyCheck;
using System.Linq;
using Xunit;

namespace Tests.V2.GenTests.CreateGenTests
{
    public class AboutConstructorPreferences
    {
        private class ImplicitDefaultConstructor
        {
            public int Property { get; set; }
        }

        [Fact]
        public void WhenThereIsAnImplicitDefaultConstructor_ItGeneratesBySettingProperties()
        {
            var gen = Gen
                .Create<ImplicitDefaultConstructor>()
                .RegisterType(Gen.Int32().Where(x => x != 0));

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
        public void WhenThereIsAnExplicitDefaultConstructor_ItGeneratesBySettingProperties()
        {
            var gen = Gen
                .Create<ExplicitDefaultConstructor>()
                .RegisterType(Gen.Int32().Where(x => x != 0));

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
        public void WhenThereIsAConstructorWithOneArgument_ItGeneratesByInvokingConstructor()
        {
            var gen = Gen.Create<ConstructorWithOneArgument>();

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.Property.Should().NotBeNull();
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
        public void WhenThereIsAConstructorWithOneArgumentAndAnotherWithTwoArguments_ItGeneratesForTheConstructorWithTwoArguments()
        {
            var gen = Gen.Create<ConstructorWithOneArgumentAndAnotherWithTwoArguments>();

            var instance = gen.SampleOne(seed: 0);

            instance.Should().NotBeNull();
            instance.Property1.Should().NotBeNull();
            instance.Property2.Should().NotBeNull();
        }
    }
}
