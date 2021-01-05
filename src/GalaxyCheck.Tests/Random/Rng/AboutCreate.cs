using FsCheck.Xunit;
using Xunit;
using R = GalaxyCheck.Random;

namespace Tests.Random.Rng
{
    public class AboutCreate
    {
        [Property]
        public void ItIsIdempotent(int seed)
        {
            var rng0 = R.Rng.Create(seed);
            var rng1 = R.Rng.Create(seed);

            Assert.Equal(rng0, rng1);
        }

        [Property]
        public void ItReturnsAnRngWithTheInitialOrder(int seed)
        {
            var rng = R.Rng.Create(seed);

            Assert.Equal(0, rng.Order);
        }
    }
}
