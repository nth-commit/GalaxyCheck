using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using Test = NebulaCheck.Test;
using Property = NebulaCheck.Property;
using Gen = NebulaCheck.Gen;
using System.Collections.Generic;

namespace Tests.V2.GenTests.ChooseGenTests
{
    public class AboutValueProduction
    {
        [Property]
        public NebulaCheck.IGen<Test> IfThereIsOneChoice_ItProducesValuesLikeTheSource() =>
            from choiceGen in DomainGen.Gen()
            from choiceWeight in Gen.Int32().GreaterThan(0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<object> SampleTraversal(GalaxyCheck.IGen<object> gen, int seed) =>
                    AboutValueProduction.SampleTraversal(gen, seed, size);

                var gen = GalaxyCheck.Gen.Choose<object>().WithChoice(choiceGen, choiceWeight);

                var sample0 = SampleTraversal(gen, seed);
                var sample1 = SampleTraversal(choiceGen, SeedUtility.Skip(seed));
                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> IfChoicesWeightsAreOne_ItProducesValuesLikeANonBiasedInt32Gen() =>
            from numberOfChoices in Gen.Int32().Between(1, 20) 
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) =>
                    AboutValueProduction.SampleTraversal(gen, seed, size);

                var range = Enumerable.Range(0, numberOfChoices);

                var gen = GalaxyCheck.Gen
                    .Choose(range
                        .Select(x => (GalaxyCheck.Gen.Constant(x), 1))
                        .ToArray());

                var int32Gen = GalaxyCheck.Gen.Int32()
                    .Between(0, numberOfChoices - 1)
                    .WithBias(GalaxyCheck.Gen.Bias.None);

                var sample0 = SampleTraversal(gen);
                var sample1 = SampleTraversal(int32Gen);
                sample0.Should().BeEquivalentTo(sample1);
            });

        private static List<T> SampleTraversal<T>(GalaxyCheck.IGen<T> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
