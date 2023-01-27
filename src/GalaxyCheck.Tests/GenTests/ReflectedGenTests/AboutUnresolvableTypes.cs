using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Gens;
using System;
using System.Collections.Immutable;
using Xunit;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutUnresolvableTypes
    {
        [Fact]
        public void ItErrorsWhenPrimitiveIsUnresolvable()
        {
            var gen = new ReflectedGen<int>(ImmutableDictionary.Create<Type, Func<IGen>>(), null);

            Action action = () => gen.SampleOne(seed: 0);

            action.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithGenErrorMessage("could not resolve type 'System.Int32'");
        }

        private class ClassWithOneProperty
        {
            public int Property { get; set; }
        }

        [Fact]
        public void ItErrorsWhenPropertyTypeIsUnresolvable()
        {
            var gen = new ReflectedGen<ClassWithOneProperty>(ImmutableDictionary.Create<Type, Func<IGen>>(), null);

            Action action = () => gen.SampleOne(seed: 0);

            action.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithGenErrorMessage("could not resolve type 'System.Int32' at path '$.Property'");
        }

        private class ClassWithOneNestedProperty
        {
            public ClassWithOneProperty Property { get; set; } = null!;
        }

        [Fact]
        public void ItErrorsWhenNestedPropertyTypeIsUnresolvable()
        {
            var gen = new ReflectedGen<ClassWithOneNestedProperty>(ImmutableDictionary.Create<Type, Func<IGen>>(), null);

            Action action = () => gen.SampleOne(seed: 0);

            action.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithGenErrorMessage("could not resolve type 'System.Int32' at path '$.Property.Property'");
        }
    }
}
