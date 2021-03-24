using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Immutable;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.InfiniteGenTests
{
    public class AboutValueProduction
    {
        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesDeterministically() =>
            from elementGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = elementGen.InfiniteOf();

                var sample = gen.SampleOne(seed: seed, size: size);

                sample.Take(10).Should().BeEquivalentTo(sample.Take(10));
            });

        /// <summary>
        /// The generation is mostly comparable, except:
        ///   - We have to give the list generator a forked seed, to simulate the forking that happens in the infinite
        ///     enumerable.
        ///   - We have to set the list's length to be a specific value using OfLength, as that short-circuits
        ///     consuming randomness for generating the list's length.
        ///   - We can only compare the root values, not the entire example space. This is because the list generator
        ///     and the infinite generator do not shrink in the same way.
        /// </summary>
        [Property]
        public NebulaCheck.IGen<Test> ItProducesValuesLikeList() =>
            from elementGen in DomainGen.Gen()
            from count in Gen.Int32().Between(0, 20)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                ImmutableList<object> Sample(GalaxyCheck.IGen<ImmutableList<object>> gen, int seed) =>
                    gen.SampleOne(seed: seed, size: size);

                var gen = elementGen.InfiniteOf();

                var listGen = elementGen.ListOf().OfCount(count);
                var infiniteGen = elementGen.InfiniteOf().Select(enumerable => enumerable.Take(count).ToImmutableList());

                var listSample = Sample(listGen, SeedUtility.Fork(seed));
                var infiniteSample = Sample(infiniteGen, seed);

                infiniteSample.Should().BeEquivalentTo(listSample);
            });
    }
}
