using FluentAssertions;
using NebulaCheck;

namespace Tests.V2.ImplementationTests.RngTests
{
    public class AboutNext
    {
        [Property]
        public IGen<Test> ItIsPure() =>
            from rng in RandomDomainGen.Rng()
            select Property.ForThese(() =>
            {
                rng.Next().Should().BeEquivalentTo(rng.Next());
            });

        [Property]
        public IGen<Test> ItIncrementsTheOrder() =>
            from rng in RandomDomainGen.Rng()
            select Property.ForThese(() =>
            {
                rng.Next().Order.Should().Be(rng.Order + 1);
            });
    }
}
