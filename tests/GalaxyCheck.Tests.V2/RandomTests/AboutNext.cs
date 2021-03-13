using FluentAssertions;
using NebulaCheck;
using NebulaCheck.Xunit;

namespace Tests.V2.RandomTests
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
