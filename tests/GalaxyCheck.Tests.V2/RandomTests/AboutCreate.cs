using FluentAssertions;
using GalaxyCheck.Internal.Random;
using NebulaCheck;
using NebulaCheck.Xunit;

namespace Tests.V2.RandomTests
{
    public class AboutCreate
    {
        [Property]
        public IGen<Test> ItIsRepeatable() =>
            from seed in Gen.Int32()
            select Property.ForThese(() =>
            {
                var rng0 = Rng.Create(seed);
                var rng1 = Rng.Create(seed);

                rng0.Should().BeEquivalentTo(rng1);
            });

        [Property]
        public IGen<Test> ItInitializesTheOrder() =>
            from seed in Gen.Int32()
            select Property.ForThese(() =>
            {
                var rng = Rng.Create(seed);

                rng.Order.Should().Be(0);
            });
    }
}
