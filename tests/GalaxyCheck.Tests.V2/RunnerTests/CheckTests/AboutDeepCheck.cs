using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.RunnerTests.CheckTests
{
    public class AboutDeepCheck
    {
        [Property]
        public NebulaCheck.IGen<Test> IfDeepCheckIsEnabled_ForAnEasilyFalsifiableProperty_ItIsNotLimitedByDeepCheckDisabled() =>
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Gen.Int32().ForAll(x => x < 1);

                var result = property.Check(seed: seed, deepCheck: true);

                result.TerminationReason.Should().NotBe(GalaxyCheck.TerminationReason.DeepCheckDisabled);
            });

        [Property]
        public NebulaCheck.IGen<Test> IfDeepCheckIsDisabled_ForAnEasilyFalsifiableProperty_ItIsLimitedByDeepCheckDisabled() =>
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Gen.Int32().ForAll(x => x < 1);

                var result = property.Check(seed: seed, deepCheck: false);

                result.TerminationReason.Should().Be(GalaxyCheck.TerminationReason.DeepCheckDisabled);
            });

        [Property]
        public NebulaCheck.IGen<Test> DefaultDeepCheckStateIsEnabled() =>
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Gen.Int32().ForAll(x => x < 1);

                var result0 = property.Check(seed: seed);
                var result1 = property.Check(seed: seed, deepCheck: true);

                result0.Counterexample!.Replay.Should().BeEquivalentTo(result1.Counterexample!.Replay);
                result0.Iterations.Should().Be(result1.Iterations);
            });

    }
}
