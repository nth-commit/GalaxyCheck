using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.RunnerTests.CheckTests
{
    public class AboutSizing
    {
        [Property]
        public NebulaCheck.IGen<Test> ItFalsifiesAPropertyThatOnlyFalsifiesAtLargerSizes() =>
            from iterations in DomainGen.Iterations(allowZero: false)
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Gen.Advanced.Create((_, size) => size.Value).ForAll(x => x < 50);

                var result = property.Check(iterations: iterations, seed: seed);

                result.Falsified.Should().BeTrue();
            });

        [Property]
        public NebulaCheck.IGen<Test> ItFalsifiesAPropertyThatOnlyFalsifiesAtSmallerSizes() =>
            from iterations in DomainGen.Iterations(allowZero: false)
            where iterations >= 2 // Otherwise the first iteration is 100
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Gen.Advanced.Create((_, size) => size.Value).ForAll(x => x > 50);

                var result = property.Check(iterations: iterations, seed: seed);

                result.Falsified.Should().BeTrue();
            });

        [Property]
        public NebulaCheck.IGen<Test> ForAConstantSize_AndANonResizingGen_ItDoesNotResize() =>
            from gen in DomainGen.Gen()
            from func in DomainGen.Predicate<object>()
            from iterations in DomainGen.Iterations()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var property = gen.ForAll(_ => true);

                var result = property.Check(iterations: iterations, seed: seed, size: size);

                result.NextParameters.Size.Value.Should().Be(size);
            });
    }
}
