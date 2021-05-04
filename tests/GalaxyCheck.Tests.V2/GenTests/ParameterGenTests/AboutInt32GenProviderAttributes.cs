using System.Reflection;
using Xunit;
using NebulaCheck;
using GalaxyCheck;
using GalaxyCheck.Gens.Injection.Int32;
using Property = NebulaCheck.Property;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;

namespace Tests.V2.GenTests.ParameterGenTests
{
    public class AboutInt32GenProviderAttributes
    {
        private static void GreaterThanEqual0Example([GreaterThanEqual(0)] int _) { }
        private static void GreaterThanEqual10Example([GreaterThanEqual(10)] int _) { }

        [Theory]
        [InlineData(nameof(GreaterThanEqual0Example), 0)]
        [InlineData(nameof(GreaterThanEqual10Example), 10)]
        public void GreaterThanEqualIsSupportedViaAnAttribute(string methodName, int expectedMin)
        {
            var property =
                from seed in DomainGen.Seed()
                from size in DomainGen.Size()
                select Property.ForThese(() =>
                {
                    List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) =>
                        AboutInt32GenProviderAttributes.SampleTraversal(gen, seed: seed, size: size);

                    var gen0 = GalaxyCheck.Gen
                        .Parameters(GetMethod(methodName))
                        .Select(x => x.Single())
                        .Cast<int>();
                    var gen1 = GalaxyCheck.Gen.Int32().GreaterThanEqual(expectedMin);

                    var sample0 = SampleTraversal(gen0);
                    var sample1 = SampleTraversal(gen1);

                    sample0.Should().BeEquivalentTo(sample1);
                });

            property.Assert();
        }

        private static void LessThanEqual0Example([LessThanEqual(0)] int _) { }
        private static void LessThanEqual10Example([LessThanEqual(10)] int _) { }

        [Theory]
        [InlineData(nameof(LessThanEqual0Example), 0)]
        [InlineData(nameof(LessThanEqual10Example), 10)]
        public void LessThanEqualIsSupportedViaAnAttribute(string methodName, int expectedMax)
        {
            var property =
                from seed in DomainGen.Seed()
                from size in DomainGen.Size()
                select Property.ForThese(() =>
                {
                    List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) =>
                        AboutInt32GenProviderAttributes.SampleTraversal(gen, seed: seed, size: size);

                    var gen0 = GalaxyCheck.Gen
                        .Parameters(GetMethod(methodName))
                        .Select(x => x.Single())
                        .Cast<int>();
                    var gen1 = GalaxyCheck.Gen.Int32().LessThanEqual(expectedMax);

                    var sample0 = SampleTraversal(gen0);
                    var sample1 = SampleTraversal(gen1);

                    sample0.Should().BeEquivalentTo(sample1);
                });

            property.Assert();
        }

        private static void Between0And5Example([Between(0, 5)] int _) { }
        private static void Between5And10Example([Between(5, 10)] int _) { }

        [Theory]
        [InlineData(nameof(Between0And5Example), 0, 5)]
        [InlineData(nameof(Between5And10Example), 5, 10)]
        public void BetweenIsSupportedViaAnAttribute(string methodName, int expectedX, int expectedY)
        {
            var property =
                from seed in DomainGen.Seed()
                from size in DomainGen.Size()
                select Property.ForThese(() =>
                {
                    List<int> SampleTraversal(GalaxyCheck.IGen<int> gen) =>
                        AboutInt32GenProviderAttributes.SampleTraversal(gen, seed: seed, size: size);

                    var gen0 = GalaxyCheck.Gen
                        .Parameters(GetMethod(methodName))
                        .Select(x => x.Single())
                        .Cast<int>();
                    var gen1 = GalaxyCheck.Gen.Int32().Between(expectedX, expectedY);

                    var sample0 = SampleTraversal(gen0);
                    var sample1 = SampleTraversal(gen1);

                    sample0.Should().BeEquivalentTo(sample1);
                });

            property.Assert();
        }

        private static MethodInfo GetMethod(string name)
        {
            return typeof(AboutInt32GenProviderAttributes).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;
        }

        private static List<T> SampleTraversal<T>(GalaxyCheck.IGen<T> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
