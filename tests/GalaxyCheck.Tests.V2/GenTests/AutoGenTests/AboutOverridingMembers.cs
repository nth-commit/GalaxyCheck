using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;


namespace Tests.V2.GenTests.AutoGenTests
{
    public class AboutOverridingMembers
    {
        private record RecordWithOneProperty(object Property);

        [Property]
        public NebulaCheck.IGen<Test> ItCanOverrideAPropertyAtTheRoot() =>
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
    }
}
