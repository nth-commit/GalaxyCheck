using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Test = NebulaCheck.Test;
using Property = NebulaCheck.Property;

namespace Tests.V2.GenTests.CharGenTests
{
    public class AboutValueProduction
    {
        [Property]
        public NebulaCheck.IGen<Test> IfCharTypeIsWhitespace_ItOnlyProducesWhitespaceCharacters() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Char(GalaxyCheck.Gen.CharType.Whitespace);

                var sample = SampleTraversal(gen, seed, size);

                sample.Should().OnlyContain(c => new Regex(@"\s").IsMatch(c.ToString()));
            });

        [Property]
        public NebulaCheck.IGen<Test> IfCharTypeIsAlphabetic_ItOnlyProducesAlphabeticCharacters() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Char(GalaxyCheck.Gen.CharType.Alphabetical);

                var sample = SampleTraversal(gen, seed, size);

                sample.Should().OnlyContain(c => new Regex(@"[A-Z]|[a-z]").IsMatch(c.ToString()));
            });

        [Property]
        public NebulaCheck.IGen<Test> IfCharTypeIsNumeric_ItOnlyProducesNumericCharacters() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Char(GalaxyCheck.Gen.CharType.Numeric);

                var sample = SampleTraversal(gen, seed, size);

                sample.Should().OnlyContain(c => new Regex(@"[0-9]").IsMatch(c.ToString()));
            });

        [Property]
        public NebulaCheck.IGen<Test> IfCharTypeIsSymbol_ItOnlyProducesSymbolCharacters() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Char(GalaxyCheck.Gen.CharType.Symbol);

                var sample = SampleTraversal(gen, seed, size);

                sample.Should().OnlyContain(c =>
                    new Regex(@"\D").IsMatch(c.ToString()) &&
                    new Regex(@"\S").IsMatch(c.ToString()));
            });

        [Property]
        public NebulaCheck.IGen<Test> IfCharTypeIsExtended_ItOnlyProducesCharactersInExtendedAsciiSet() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Char(GalaxyCheck.Gen.CharType.Extended);

                var sample = SampleTraversal(gen, seed, size);

                sample.Should().OnlyContain(c => c >= 128);
            });

        [Property]
        public NebulaCheck.IGen<Test> IfCharTypeIsControl_ItOnlyProducesCharactersInControlAsciiSet() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Char(GalaxyCheck.Gen.CharType.Control);

                var sample = SampleTraversal(gen, seed, size);

                sample.Should().OnlyContain(c => c <= 37 || c == 127);
            });

        private static List<char> SampleTraversal(GalaxyCheck.IGen<char> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
