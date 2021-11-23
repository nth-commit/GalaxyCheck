using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.SelectManyGenTests
{
    public class AboutExhaustion
    {
        [Property(Iterations = 1)]
        public void IfTheSourceGeneratorExhaustsWhenTheLeftGeneratorExhausts([Seed] int seed, [Size] int size)
        {
            var genLeft = GalaxyCheck.Gen.Int32().Where(x => false);
            var genRight = GalaxyCheck.Gen.Int32();
            var gen = genLeft.SelectMany(_ => genRight);

            Action test = () => gen.SampleOne(seed: seed, size: size);

            test.Should().Throw<GalaxyCheck.Exceptions.GenExhaustionException>();
        }

        [Property(Iterations = 1)]
        public void IfTheSourceGeneratorExhaustsWhenTheRightGeneratorExhausts([Seed] int seed, [Size] int size)
        {
            var genLeft = GalaxyCheck.Gen.Int32();
            var genRight = GalaxyCheck.Gen.Int32().Where(x => false);
            var gen = genLeft.SelectMany(_ => genRight);

            Action test = () => gen.SampleOne(seed: seed, size: size);

            test.Should().Throw<GalaxyCheck.Exceptions.GenExhaustionException>();
        }
    }
}
