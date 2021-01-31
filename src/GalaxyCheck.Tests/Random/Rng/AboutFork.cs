using Xunit;
using R = GalaxyCheck.Internal.Random;

namespace Tests.Random.Rng
{
    // TODO: Make initial RNGs arbitrary, hard to do with fast-check as it's hard to write a custom generator...
    public class AboutFork
    {
        [Fact]
        public void ItIsPure()
        {
            var rng = R.Rng.Spawn().Next().Next(); // TODO: Arbitrary number of nexts in injected Rng;

            Assert.Equal(rng.Fork(), rng.Fork());
        }

        [Fact]
        public void ItResetsOrderToZero()
        {
            var rng = R.Rng.Spawn().Next().Next(); // TODO: Arbitrary number of nexts in injected Rng

            Assert.Equal(0, rng.Fork().Order);
        }

        [Fact]
        public void ItChangesTheFamily()
        {
            var rng = R.Rng.Spawn().Next().Next(); // TODO: Arbitrary number of nexts in injected Rng;

            Assert.NotEqual(rng.Family, rng.Fork().Family);
        }
    }
}
