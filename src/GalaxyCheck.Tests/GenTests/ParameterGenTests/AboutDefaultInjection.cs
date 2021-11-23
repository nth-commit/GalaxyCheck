using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.ParameterGenTests
{
    public class AboutDefaultInjection
    {
        private static void MethodWithInt32Parameter(int parameter)
        {
        }

        [Property]
        public void ItGeneratesInt32sLikeTheDefaultInt32Gen([Seed] int seed, [Size] int size)
        {
            List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) =>
                AboutDefaultInjection.SampleTraversal(gen, seed: seed, size: size);

            var gen0 = GalaxyCheck.Gen
                .Parameters(GetMethod(nameof(MethodWithInt32Parameter)))
                .Select(x => x.Single())
                .Cast<int>();
            var gen1 = GalaxyCheck.Gen.Int32();

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample0.Should().BeEquivalentTo(sample1);
        }

        private static void MethodWithCharParameter(char parameter)
        {
        }

        [Property]
        public void ItGeneratesCharsLikeTheDefaultCharGen([Seed] int seed, [Size] int size)
        {
            List<char> SampleTraversal(GalaxyCheck.IGen<char> gen) =>
                AboutDefaultInjection.SampleTraversal(gen, seed: seed, size: size);

            var gen0 = GalaxyCheck.Gen
                .Parameters(GetMethod(nameof(MethodWithCharParameter)))
                .Select(x => x.Single())
                .Cast<char>();
            var gen1 = GalaxyCheck.Gen.Char();

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample0.Should().BeEquivalentTo(sample1);
        }

        private static void MethodWithStringParameter(string parameter)
        {
        }

        [Property]
        public void ItGeneratesStringsLikeTheDefaultStringGen([Seed] int seed, [Size] int size)
        {
            List<string> SampleTraversal(GalaxyCheck.IGen<string> gen) =>
                AboutDefaultInjection.SampleTraversal(gen, seed: seed, size: size);

            var gen0 = GalaxyCheck.Gen
                .Parameters(GetMethod(nameof(MethodWithStringParameter)))
                .Select(x => x.Single())
                .Cast<string>();
            var gen1 = GalaxyCheck.Gen.String();

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample0.Should().BeEquivalentTo(sample1);
        }

        private record SimpleObject(string A, string B, string C);

        private record ComplexObject(
            int Int32,
            List<int> Int32s,
            SimpleObject SimpleObject);

        private static void MethodWithComplexObjectParameter(ComplexObject parameter)
        {
        }

        [Property]
        public void ItCanGenerateSimpleObjects([Seed] int seed, [Size] int size)
        {
            var gen = GalaxyCheck.Gen
                .Parameters(GetMethod(nameof(MethodWithComplexObjectParameter)))
                .Select(x => x.Single())
                .Cast<ComplexObject>();

            var sample = gen.SampleOne(seed: seed, size: size);

            sample.Int32s.Should().NotBeNull();
            sample.SimpleObject.Should().NotBeNull();
        }

        private static MethodInfo GetMethod(string name)
        {
            return typeof(AboutDefaultInjection).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;
        }

        private static List<T> SampleTraversal<T>(GalaxyCheck.IGen<T> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
