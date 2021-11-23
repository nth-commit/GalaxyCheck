using FluentAssertions;
using NebulaCheck;

namespace Tests.V2.ImplementationTests.RngTests
{
    public class AboutFork
    {
        [Property]
        public IGen<Test> ItIsPure() =>
            from rng in RandomDomainGen.Rng()
            select Property.ForThese(() =>
            {
                rng.Fork().Should().BeEquivalentTo(rng.Fork());
            });

        [Property]
        public IGen<Test> ItResetsOrderToZero() =>
            from rng in RandomDomainGen.Rng()
            select Property.ForThese(() =>
            {
                rng.Fork().Order.Should().Be(0);
            });

        [Property]
        public IGen<Test> ItReassignsFamily() =>
            from rng in RandomDomainGen.Rng()
            select Property.ForThese(() =>
            {
                rng.Fork().Family.Should().NotBe(rng.Family);
            });
    }
}
