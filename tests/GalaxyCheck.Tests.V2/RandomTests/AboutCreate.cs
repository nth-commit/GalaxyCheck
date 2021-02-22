using NebulaCheck;
using Xunit;

using Rng = GalaxyCheck.Internal.Random.Rng;

namespace Tests.V2.RandomTests
{
    public class AboutCreate
    {
        [Fact]
        public void ItIsRepeatable() => Gen
            .Int32()
            .NoShrink()
            .ForAll(seed =>
            {
                var rng0 = Rng.Create(seed);
                var rng1 = Rng.Create(seed);

                Assert.Equal(rng0, rng1);
            })
            .Assert();

        [Fact]
        public void ItInitializesTheOrder() => Gen
            .Int32()
            .NoShrink()
            .ForAll(seed =>
            {
                var rng = Rng.Create(seed);

                Assert.Equal(0, rng.Order);
            })
            .Assert();
    }
}
