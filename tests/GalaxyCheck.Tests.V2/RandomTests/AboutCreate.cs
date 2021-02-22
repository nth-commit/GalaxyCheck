using NebulaCheck;
using Xunit;
using Rng = GalaxyCheck.Internal.Random.Rng;

namespace Tests.V2.RandomTests
{
    public class AboutCreate
    {
        [Fact]
        public void ItIsRepeatable()
        {
            var checkResult = Gen
                .Int32()
                .NoShrink()
                .ForAll(seed =>
                {
                    var rng0 = Rng.Create(seed);
                    var rng1 = Rng.Create(seed);

                    Assert.Equal(rng0, rng1);
                })
                .Check();

            Assert.False(checkResult.Falsified);
        }

        [Fact]
        public void ItInitializesTheOrder()
        {
            var checkResult = Gen
                .Int32()
                .NoShrink()
                .ForAll(seed =>
                {
                    var rng = Rng.Create(seed);

                    Assert.Equal(0, rng.Order);
                })
                .Check();

            Assert.False(checkResult.Falsified);
        }
    }
}
