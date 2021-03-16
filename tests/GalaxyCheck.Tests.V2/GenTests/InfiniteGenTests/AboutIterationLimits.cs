using GalaxyCheck;
using GalaxyCheck.Internal.ExampleSpaces;
using NebulaCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;

namespace Tests.V2.GenTests.InfiniteGenTests
{
    public class AboutIterationLimits
    {
        private const int ExpectedDefaultIterationLimit = 1000;

        [Fact]
        public void AGeneratedEnumerableHasTheDefaultIterationLimitIfNotConfiguredOtherwise()
        {
            NebulaCheck.Property<object> property = new Property(
                from elementGen in DomainGen.Gen()
                from seed in DomainGen.Seed()
                from size in DomainGen.Size()
                select Property.ForThese(() =>
                {
                    var gen = elementGen.InfiniteOf().Select(EnsureSourceCanShrink);

                    var sample = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size);

                    AssertLimit(sample, ExpectedDefaultIterationLimit);
                }));

            property.Assert(iterations: 10);
        }

        [Fact]
        public void AGeneratedEnumerableHasTheGivenLimit()
        {
            NebulaCheck.Property<object> property = new Property(
                from elementGen in DomainGen.Gen()
                from limit in Gen.Int32().Between(1, 1000)
                from seed in DomainGen.Seed()
                from size in DomainGen.Size()
                select Property.ForThese(() =>
                {
                    var gen = elementGen.InfiniteOf().WithIterationLimit(limit).Select(EnsureSourceCanShrink);

                    var sample = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size);

                    AssertLimit(sample, limit);
                }));

            property.Assert(iterations: 10);
        }

        [Fact]
        public void AGeneratedEnumerableWithoutALimitNeverThrows()
        {
            NebulaCheck.Property<object> property = new Property(
                from elementGen in DomainGen.Gen()
                from seed in DomainGen.Seed()
                from size in DomainGen.Size()
                select Property.ForThese(() =>
                {
                    var gen = elementGen.InfiniteOf().WithoutIterationLimit().Select(EnsureSourceCanShrink);

                    var sample = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size);

                    AssertUnlimited(sample);
                }));

            property.Assert(iterations: 10);
        }

        private static void AssertLimit<T>(IExampleSpace<IEnumerable<T>> exampleSpace, int expectedLimit)
        {
            exampleSpace.Traverse().Take(10).ToList().ForEach(enumerable =>
            {
                AssertLimit(enumerable, expectedLimit);
            });
        }

        private static void AssertLimit<T>(IEnumerable<T> enumerable, int expectedLimit)
        {
            // It doesn't thrown when the limit is hit
            enumerable.Take(expectedLimit).ToList();

            // It throws when it exceeds the limit
            Action throwing = () => enumerable.Take(expectedLimit + 1).ToList();
            var exception = Assert.Throws<GalaxyCheck.Exceptions.GenLimitExceededException>(throwing);
            Assert.Equal("Infinite enumerable exceeded iteration limit. This is a built-in safety mechanism to prevent hanging tests. Use IInfiniteGen.WithIterationLimit or IInfiniteGen.WithoutIterationLimit to modify this limit.", exception.Message);
        }

        private static void AssertUnlimited<T>(IExampleSpace<IEnumerable<T>> exampleSpace)
        {
            exampleSpace.Traverse().Take(10).ToList().ForEach(enumerable =>
            {
                AssertUnlimited(enumerable);
            });
        }

        private static void AssertUnlimited<T>(IEnumerable<T> enumerable)
        {
            enumerable.Take(ExpectedDefaultIterationLimit + 1).ToList();
        }

        private static IEnumerable<T> EnsureSourceCanShrink<T>(IEnumerable<T> source)
        {
            // Ensure the enumerable has a shrink by enumerating at least one elements. Then it will be able to at
            // least shrink to the enumerable that repeats a single element.
            source.Take(1).ToList();
            return source;
        }
    }
}
