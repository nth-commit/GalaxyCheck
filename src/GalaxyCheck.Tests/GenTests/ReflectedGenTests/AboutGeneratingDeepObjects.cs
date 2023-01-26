using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Xunit;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutGeneratingDeepObjects
    {
        private class ClassWithOneProperty
        {
            public object Property { get; set; } = null!;
        }

        private class ClassWithOneNestedProperty
        {
            public ClassWithOneProperty Property { get; set; } = null!;
        }   

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateAnObjectWithANestedProperty() =>
            from value in DomainGen.Any()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .Create<ClassWithOneNestedProperty>()
                    .OverrideMember(x => x.Property.Property, GalaxyCheck.Gen.Constant(value));

                var instance = gen.SampleOne(seed: seed, size: size);

                instance.Should().NotBeNull();
                instance.Property.Should().NotBeNull();
                instance.Property.Property.Should().Be(value);
            });

        private class ClassWithRecursiveProperty
        {
            public ClassWithRecursiveProperty Property { get; set; } = null!;
        }

        [Property]
        public NebulaCheck.IGen<Test> IfTheClassHasARecursiveProperty_ItErrors() =>
            from value in DomainGen.Any()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Create<ClassWithRecursiveProperty>();

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator ReflectedGen: detected circular reference on type '*ClassWithRecursiveProperty' at path '$.Property'");
            });

        [Fact]
        public void ItCanHandleDeepErrors()
        {
            // Arrange
            var gen = GalaxyCheck.Gen.Create<ClassWithOneNestedProperty>()
                .OverrideMember(x => x.Property.Property, GalaxyCheck.Gen.Advanced.Error<object>("Error", "Error"));
            
            Action action = () => gen.SampleOne(seed: 0, size: 0);

            action.Should()
                .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                .WithMessage("Error while running generator Error at path '$.Property': Error");
        }
        
    }
}
