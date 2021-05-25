using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.CastGenTests
{
    public class AboutExhaustion
    {
        [Property(Iterations = 1)]
        public void IfTheSourceGeneratorExhaustsThenTheCastGeneratorExhausts([Seed] int seed, [Size] int size)
        {
            var gen = GalaxyCheck.Gen.Int32().Where(x => false).Cast<object>();

            Action test = () => gen.SampleOne(seed: seed, size: size);

            test.Should().Throw<GalaxyCheck.Exceptions.GenExhaustionException>();
        }
    }
}
