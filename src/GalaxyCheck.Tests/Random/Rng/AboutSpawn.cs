using System.Linq;
using Xunit;
using R = GalaxyCheck.Random;

namespace GalaxyCheck.Tests.Random.Rng
{
    public class AboutSpawn
    {
        [Fact]
        public void ItReturnsRngsWithReasonableRandomness()
        {
            var sampleSize = 10;
            var seeds = Enumerable.Range(0, sampleSize).Select(_ => R.Rng.Spawn()).Select(rng => rng.Seed).ToList();

            // It's pretty unlikely that the same seed should ever be generated, given such a low sample size, but
            // allow just one duplication to side-step any flakiness.
            Assert.True(seeds.Distinct().Count() >= sampleSize - 1);
        }

        [Fact]
        public void ItReturnsAnRngWithTheInitialOrder()
        {
            var rng = R.Rng.Spawn();

            Assert.Equal(0, rng.Order);
        }
    }
}
