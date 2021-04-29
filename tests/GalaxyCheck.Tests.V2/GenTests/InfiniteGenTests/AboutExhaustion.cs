using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.InfiniteGenTests
{
    public class AboutExhaustion
    {
        [Property(Iterations = 10)]
        public NebulaCheck.IGen<Test> ItExhaustsWithAnImpossiblePredicate() =>
            from elementGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = elementGen.Where(_ => false).InfiniteOf();

                Action test = () => gen.SampleOne(seed: seed, size: size).Take(1).ToList();

                test.Should().Throw<GalaxyCheck.Exceptions.GenExhaustionException>();
            });
    }
}
