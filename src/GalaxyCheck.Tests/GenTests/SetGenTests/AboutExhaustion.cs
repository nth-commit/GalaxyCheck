using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.SetGenTests
{
    public class AboutExhaustion
    {
        [Property(Iterations = 1)]
        public void ItExhaustsWhenGeneratingASetOfAnImpossibleSize([Seed] int seed, [Size] int size)
        {
            var elementGen = GalaxyCheck.Gen.Boolean();
            var gen = GalaxyCheck.Gen.Set(elementGen).WithCount(3);

            Action test = () => gen.SampleOne(seed: seed, size: size);

            test.Should().Throw<GalaxyCheck.Exceptions.GenExhaustionException>();
        }
    }
}
