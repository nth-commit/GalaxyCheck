using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.InfiniteGenTests
{
    public class AboutShrinking
    {
        [Property]
        public NebulaCheck.IGen<Test> WhenItIsNotEnumerated_ItWillNotShrink() =>
            from elementGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = elementGen.InfiniteOf();

                var sample = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size);

                sample.Traverse().Should().ContainSingle();
            });

        [Property]
        public NebulaCheck.IGen<Test> WhenItIsEnumeratedOnce_AndTheElementGeneratorDoesNotShrink_ItWillShrinkOnce() =>
            from elementGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = elementGen.NoShrink().InfiniteOf().Select(enumerable =>
                {
                    enumerable.Take(1).ToList();
                    return enumerable;
                });

                var sample = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size);

                sample.Traverse().Should().HaveCount(2);
            });


        [Property]
        public NebulaCheck.IGen<Test> WhenItIsEnumeratedOnce_ItWillShrinkToTheSmallestElementRepeated() =>
            from elementGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var elementMin = elementGen.Minimum(seed: ForkSeed(seed), size: size);

                var infiniteGen = elementGen.InfiniteOf().Select(enumerable =>
                {
                    enumerable.Take(1).ToList();
                    return enumerable;
                });

                var minimum = infiniteGen.Minimum(seed: seed, size: size);
                
                minimum.Take(10).Should().BeEquivalentTo(Enumerable.Range(0, 10).Select(_ => elementMin));
            });

        private static int ForkSeed(int seed)
        {
            return GalaxyCheck.Internal.Random.Rng.Create(seed).Fork().Seed;
        }
    }
}
