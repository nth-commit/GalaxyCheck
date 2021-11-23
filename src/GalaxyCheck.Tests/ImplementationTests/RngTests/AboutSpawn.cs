using FluentAssertions;
using GalaxyCheck.Gens.Parameters.Internal;
using NebulaCheck;
using System.Linq;

namespace Tests.V2.ImplementationTests.RngTests
{
    public class AboutSpawn
    {
        [Property]
        public IGen<Test> ItReturnsRngsWithReasonableRandomness() => Property.Nullary(() =>
        {
            var sampleSize = 10;
            var seeds = Enumerable.Range(0, sampleSize).Select(_ => Rng.Spawn()).Select(rng => rng.Seed).ToList();

            // It's pretty unlikely that the same seed should ever be generated, given such a low sample size, but
            // allow just one duplication to side-step any flakiness.
            seeds.Distinct().Should().HaveCountGreaterOrEqualTo(sampleSize - 1);
        });

        [Property]
        public IGen<Test> ItInitializesTheOrder() => Property.Nullary(() =>
        {
            var rng = Rng.Spawn();

            rng.Order.Should().Be(0);
        });
    }
}
