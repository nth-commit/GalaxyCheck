using FluentAssertions;
using GalaxyCheck.Gens.Parameters.Internal;
using NebulaCheck;

namespace Tests.V2.ImplementationTests.RngTests
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
