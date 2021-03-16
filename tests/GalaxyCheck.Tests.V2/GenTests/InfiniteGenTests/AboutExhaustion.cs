using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Xunit;
using Property = NebulaCheck.Property;

namespace Tests.V2.GenTests.InfiniteGenTests
{
    public class AboutExhaustion
    {
        [Fact]
        public void ItExhaustsWithAnImpossiblePredicate()
        {
            NebulaCheck.Property<object> property = new Property(
                from elementGen in DomainGen.Gen()
                from seed in DomainGen.Seed()
                from size in DomainGen.Size()
                select Property.ForThese(() =>
                {
                    var gen = elementGen.Where(_ => false).InfiniteOf();

                    Action test = () => gen.SampleOne(seed: seed, size: size).Take(1).ToList();

                    test.Should().Throw<GalaxyCheck.Exceptions.GenExhaustionException>();
                }));

            property.Assert(iterations: 10);
        }
    }
}
