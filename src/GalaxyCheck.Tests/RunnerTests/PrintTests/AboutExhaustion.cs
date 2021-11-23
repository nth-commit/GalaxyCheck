using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.RunnerTests.PrintTests
{
    public class AboutExhaustion
    {
        [Property(Iterations = 1)]
        public void ItCanExhaust([Seed] int seed, [Size] int size)
        {
            var property = GalaxyCheck.Gen.Int32().Where(x => false).ForAll(_ => true);

            Action test = () => property.Advanced.Print(seed: seed, size: size);

            test.Should().Throw<GalaxyCheck.Exceptions.GenExhaustionException>();
        }
    }
}
