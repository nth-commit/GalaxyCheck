using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.RunnerTests.CheckTests
{
    public class AboutExhaustion
    {
        [Property(Iterations = 1)]
        public void ItExhaustsWhenGenerationIsImpossible([Seed] int seed, [Size] int size)
        {
            var property = GalaxyCheck.Gen.Int32().Where(x => false).ForAll(_ => true);

            Action test = () => property.Check(seed: seed, size: size, deepCheck: false);

            test.Should().Throw<GalaxyCheck.Exceptions.GenExhaustionException>();
        }

        [Property(Iterations = 1)]
        public void ItExhaustsWhenPreconditionIsImpossible([Seed] int seed, [Size] int size)
        {
            var property = GalaxyCheck.Gen.Int32().ForAll(_ => GalaxyCheck.Property.Precondition(false));

            Action test = () => property.Check(seed: seed, size: size, deepCheck: false);

            test.Should().Throw<GalaxyCheck.Exceptions.GenExhaustionException>();
        }
    }
}
