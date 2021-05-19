using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.ParameterGenTests
{
    public class AboutCustomGenAttributes
    {
        private static readonly GalaxyCheck.IGen<string> SnakeDialogueGen = GalaxyCheck.Gen
            .String()
            .FromCharacters(new[] { 's' })
            .WithLengthBetween(3, 20)
            .ListOf()
            .Select(words => string.Join(' ', words));

        private class SnakeDialogueAttribute : GalaxyCheck.GenAttribute
        {
            public override GalaxyCheck.IGen Value => SnakeDialogueGen;
        }

        private static void PropertyOfSnakeDialog([SnakeDialogue] string snekSpek)
        {
            // ssssssssss sssssssssss sss
        }

        [Property]
        public void ItUsesTheProvidedGenToGenerateTheParameter([Seed] int seed, [Size] int size)
        {
            List<string> SampleTraversal(GalaxyCheck.IGen<string> gen) =>
                AboutCustomGenAttributes.SampleTraversal(gen, seed: seed, size: size);

            var gen = GalaxyCheck.Gen
                .Parameters(GetMethod(nameof(PropertyOfSnakeDialog)))
                .Select(x => x.Single())
                .Cast<string>();

            var sample0 = SampleTraversal(gen);
            var sample1 = SampleTraversal(SnakeDialogueGen);

            sample0.Should().BeEquivalentTo(sample1);
        }

        private static MethodInfo GetMethod(string name)
        {
            return typeof(AboutCustomGenAttributes).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;
        }

        private static List<T> SampleTraversal<T>(GalaxyCheck.IGen<T> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
