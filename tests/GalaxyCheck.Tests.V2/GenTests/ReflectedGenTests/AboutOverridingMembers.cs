﻿using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutOverridingMembers
    {
        private record RecordWithOneProperty(object Property);

        private class ClassWithOneConstructorArgument
        {
            public object Property { get; }

            public ClassWithOneConstructorArgument(object property)
            {
                Property = property;
            }
        }

        [Property]
        public NebulaCheck.IGen<Test> ItCanOverrideAnExternalInitProperty() =>
            from value in DomainGen.Any()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .Create<RecordWithOneProperty>()
                    .OverrideMember(x => x.Property, GalaxyCheck.Gen.Constant(value));

                var instance = gen.SampleOne(seed: seed, size: size);

                instance.Should().NotBeNull();
                instance.Property.Should().Be(value);
            });

        private record RecordWithOneNestedProperty(RecordWithOneProperty Property);

        [Property]
        public NebulaCheck.IGen<Test> ItCanOverrideANestedExternalInitProperty() =>
            from value in DomainGen.Any()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .Create<RecordWithOneNestedProperty>()
                    .OverrideMember(x => x.Property.Property, GalaxyCheck.Gen.Constant(value));

                var instance = gen.SampleOne(seed: seed, size: size);

                instance.Should().NotBeNull();
                instance.Property.Property.Should().Be(value);
            });

        private record RecordWithAMethod(object Property)
        {
            public object Method() => null!;
        }

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenExpressionIsNotMemberAccess() =>
            from memberGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .Create<RecordWithAMethod>()
                    .OverrideMember(x => x.Method(), memberGen);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator ReflectedGen: expression 'x => x.Method()' was invalid, an overridding expression may only contain member access");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenExpressionIsNotMemberAccess_AndACorrectOverrideWasSpecifiedAfterwards() =>
            from memberGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .Create<RecordWithAMethod>()
                    .OverrideMember(x => x.Method(), memberGen)
                    .OverrideMember(x => x.Property, memberGen);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator ReflectedGen: expression 'x => x.Method()' was invalid, an overridding expression may only contain member access");
            });

        private class ClassWithNonDefaultConstructor
        {
            public object Property { get; }

            public ClassWithNonDefaultConstructor(object property)
            {
                Property = property;
            }
        }

        [Property(Skip = "Future validation")]
        public NebulaCheck.IGen<Test> ItErrorsWhenMemberAccessIsCircumventedByConstructor() =>
            from memberGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .Create<ClassWithNonDefaultConstructor>()
                    .OverrideMember(x => x.Property, memberGen);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator ReflectedGen: expression 'x => x.Property' targeted a valid member, but the type did not have a default constructor, so the member override would be ignored");
            });
    }
}
