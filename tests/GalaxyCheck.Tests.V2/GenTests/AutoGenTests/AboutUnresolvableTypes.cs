using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Gens;
using System;
using System.Collections.Immutable;
using Xunit;

namespace Tests.V2.GenTests.AutoGenTests
{
    public class AboutUnresolvableTypes
    {
        [Fact]
        public void ItErrorsWhenPrimitiveIsUnresolvable()
        {
            var gen = new AutoGen<int>(
                ImmutableDictionary.Create<Type, IGen>(),
                ImmutableList.Create<AutoGenMemberOverride>());

            Action action = () => gen.SampleOne(seed: 0);

            action.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator AutoGen: could not resolve type 'System.Int32'");
        }

        private class ClassWithOneProperty
        {
            public int Property { get; set; }
        }

        [Fact]
        public void ItErrorsWhenPropertyTypeIsUnresolvable()
        {
            var gen = new AutoGen<ClassWithOneProperty>(
                ImmutableDictionary.Create<Type, IGen>(),
                ImmutableList.Create<AutoGenMemberOverride>());

            Action action = () => gen.SampleOne(seed: 0);

            action.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator AutoGen: could not resolve type 'System.Int32' at path '$.Property'");
        }

        private class ClassWithOneNestedProperty
        {
            public ClassWithOneProperty Property { get; set; } = null!;
        }

        [Fact]
        public void ItErrorsWhenNestedPropertyTypeIsUnresolvable()
        {
            var gen = new AutoGen<ClassWithOneNestedProperty>(
                ImmutableDictionary.Create<Type, IGen>(),
                ImmutableList.Create<AutoGenMemberOverride>());

            Action action = () => gen.SampleOne(seed: 0);

            action.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator AutoGen: could not resolve type 'System.Int32' at path '$.Property.Property'");
        }
    }
}
