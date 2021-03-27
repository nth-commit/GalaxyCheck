using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Gens;
using System;
using System.Collections.Immutable;
using Xunit;

namespace Tests.V2.GenTests.CreateGenTests
{
    public class AboutUnresolvableTypes
    {
        private class ClassWithOneProperty
        {
            public int Property { get; set; }
        }

        [Fact]
        public void ItErrorsWhenPropertyTypeIsUnresolvable()
        {
            var gen = new CreateGen<ClassWithOneProperty>(ImmutableDictionary.Create<Type, IGen>());

            Action action = () => gen.SampleOne(seed: 0);

            action.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator CreateGen: could not resolve type 'System.Int32' at path '$'");
        }

        private class ClassWithOneNestedProperty
        {
            public ClassWithOneProperty Property { get; set; } = null!;
        }

        [Fact]
        public void ItErrorsWhenNestedPropertyTypeIsUnresolvable()
        {
            var gen = new CreateGen<ClassWithOneNestedProperty>(ImmutableDictionary.Create<Type, IGen>());

            Action action = () => gen.SampleOne(seed: 0);

            action.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator CreateGen: could not resolve type 'System.Int32' at path '$.Property'");
        }
    }
}
