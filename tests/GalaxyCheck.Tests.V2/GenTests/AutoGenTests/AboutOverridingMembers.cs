using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;


namespace Tests.V2.GenTests.AutoGenTests
{
    public class AboutOverridingMembers
    {
        private record RecordWithOneProperty(object Property);

        [Property]
        public NebulaCheck.IGen<Test> ItCanOverrideAProperty() =>
            from value in DomainGen.Any()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .Auto<RecordWithOneProperty>()
                    .OverrideMember(x => x.Property, GalaxyCheck.Gen.Constant(value));

                var instance = gen.SampleOne(seed: seed, size: size);

                instance.Should().NotBeNull();
                instance.Property.Should().Be(value);
            });

        private record RecordWithOneNestedProperty(RecordWithOneProperty Property);

        [Property]
        public NebulaCheck.IGen<Test> ItCanOverrideANestedProperty() =>
            from value in DomainGen.Any()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .Auto<RecordWithOneNestedProperty>()
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
                    .Auto<RecordWithAMethod>()
                    .OverrideMember(x => x.Method(), memberGen);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator AutoGen: expression 'x => x.Method()' was invalid, an overridding expression may only contain member access");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenExpressionIsNotMemberAccess_AndACorrectOverrideWasSpecifiedAfterwards() =>
            from memberGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .Auto<RecordWithAMethod>()
                    .OverrideMember(x => x.Method(), memberGen)
                    .OverrideMember(x => x.Property, memberGen);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator AutoGen: expression 'x => x.Method()' was invalid, an overridding expression may only contain member access");
            });
    }
}
