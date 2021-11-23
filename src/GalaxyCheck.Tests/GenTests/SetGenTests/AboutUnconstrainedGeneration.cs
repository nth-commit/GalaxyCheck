using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.SetGenTests
{
    public class AboutUnconstrainedGeneration
    {
        [Property(Iterations = 10)]
        public void ItCanGenerateABooleanSet([Seed] int seed, [Size] int size)
        {
            var elementGen = GalaxyCheck.Gen.Boolean();
            var gen = GalaxyCheck.Gen
                .Set(elementGen)
                .Where(set => set.Count > 0); // Ensure it doesn't always return an empty set, without explicitly constraining it

            var sample = gen.SampleOneTraversal(seed: seed, size: size);

            sample.Should().OnlyContain(set => set.Distinct().SequenceEqual(set));
        }
    }
}
