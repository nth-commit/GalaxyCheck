using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.CreateGenTests
{
    public class AboutGeneratingPrimitives
    {
        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateInt32s() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) =>
                    AboutGeneratingPrimitives.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<int>();
                var gen1 = GalaxyCheck.Gen.Int32();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateChars() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<char> SampleTraversal(GalaxyCheck.IGen<char> gen) =>
                    AboutGeneratingPrimitives.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<char>();
                var gen1 = GalaxyCheck.Gen.Char();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateStrings() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<string> SampleTraversal(GalaxyCheck.IGen<string> gen) =>
                    AboutGeneratingPrimitives.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Create<string>();
                var gen1 = GalaxyCheck.Gen.String();

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        private static List<T> SampleTraversal<T>(GalaxyCheck.IGen<T> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
