using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using System.Collections.Generic;
using Test = NebulaCheck.Test;
using Property = NebulaCheck.Property;
using Gen = NebulaCheck.Gen;

namespace Tests.V2.GenTests.StringGenTests
{
    public class AboutValueProduction
    {
        [Property]
        public NebulaCheck.IGen<Test> IfTheCharacterSelectionStrategyIsCharType_ItProducesValuesLikeListOfChar() =>
            from charType in TestGen.CharType()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<string> SampleTraversal(GalaxyCheck.IGen<string> gen) =>
                    AboutValueProduction.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen
                    .String()
                    .FromCharacters(charType);

                var gen1 = GalaxyCheck.Gen
                    .Char(charType)
                    .ListOf()
                    .WithCountBetween(0, 100)
                    .Select(cs => new string(cs.ToArray()));

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> IfTheCharacterSelectionStrategyIsFromEnumerable_ItProducesValuesLikeListOfElement() =>
            from chars in Gen.Char().ListOf().WithCountGreaterThan(0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<string> SampleTraversal(GalaxyCheck.IGen<string> gen) =>
                    AboutValueProduction.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen
                    .String()
                    .FromCharacters(chars);

                var gen1 = GalaxyCheck.Gen
                    .Element(chars)
                    .ListOf()
                    .WithCountBetween(0, 100)
                    .Select(cs => new string(cs.ToArray()));

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> IfTheStringLengthIsRanged_ItProducesValuesLikeListOfCharConstrainedInTheSameWay() =>
            from lengths in Gen.Int32().Between(0, 20).Two()
            from lengthBias in DomainGen.Bias()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<string> SampleTraversal(GalaxyCheck.IGen<string> gen) =>
                    AboutValueProduction.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen
                    .String()
                    .WithLengthBetween(lengths.Item1, lengths.Item2)
                    .WithLengthBias(lengthBias);

                var gen1 = GalaxyCheck.Gen
                    .Char()
                    .ListOf()
                    .WithCountBetween(lengths.Item1, lengths.Item2)
                    .WithCountBias(lengthBias)
                    .Select(cs => new string(cs.ToArray()));

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        [Property]
        public NebulaCheck.IGen<Test> IfTheStringLengthIsSpecific_ItProducesValuesLikeListOfCharConstrainedInTheSameWay() =>
            from length in Gen.Int32().Between(0, 20)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<string> SampleTraversal(GalaxyCheck.IGen<string> gen) =>
                    AboutValueProduction.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen
                    .String()
                    .WithLength(length);

                var gen1 = GalaxyCheck.Gen
                    .Char()
                    .ListOf()
                    .WithCount(length)
                    .Select(cs => new string(cs.ToArray()));

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        private static List<string> SampleTraversal(GalaxyCheck.IGen<string> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();

        private static class TestGen
        {
            public static NebulaCheck.IGen<GalaxyCheck.Gen.CharType> CharType()
            {
                var allCharTypes = new[]
                {
                    GalaxyCheck.Gen.CharType.Whitespace,
                    GalaxyCheck.Gen.CharType.Alphabetical,
                    GalaxyCheck.Gen.CharType.Numeric,
                    GalaxyCheck.Gen.CharType.Symbol,
                    GalaxyCheck.Gen.CharType.Extended,
                    GalaxyCheck.Gen.CharType.Control
                };

                return Gen
                    .Element(allCharTypes)
                    .ListOf()
                    .WithCountGreaterThan(0)
                    .Select(xs => xs.Aggregate((GalaxyCheck.Gen.CharType)0, (acc, curr) => acc | curr));
            }
        }
    }
}
