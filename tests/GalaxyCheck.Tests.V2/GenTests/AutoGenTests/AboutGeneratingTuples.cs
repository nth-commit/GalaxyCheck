using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.AutoGenTests
{
    public class AboutGeneratingTuples
    {
        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateATwoTuple() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen
                    .AutoFactory()
                    .RegisterType(GalaxyCheck.Gen.Int32().Where(x => x != 0))
                    .Create<(int x, int y)>();

                var instance = gen.SampleOne(seed: 0);

                instance.Should().NotBeNull();
                instance.x.Should().NotBe(0);
                instance.y.Should().NotBe(0);
            });
    }
}
