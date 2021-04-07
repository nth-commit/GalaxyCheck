using GalaxyCheck;
using FluentAssertions;
using NebulaCheck;
using System.Linq;
using Test = NebulaCheck.Test;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;

namespace Tests.V2.PropertyTests.ActionPropertyTests
{
    public class AboutArity
    {
        [Property]
        public NebulaCheck.IGen<Test> ANullaryPropertyHasAnArityOfZero() =>
            from action in DomainGen.Action()
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Property.Nullary(action);

                var sample = property.SampleOne(seed: seed);

                sample.Arity.Should().Be(0);
            });

        [Property]
        public NebulaCheck.IGen<Test> AUnaryPropertyHasAnArityOfOne() =>
            from gen in DomainGen.Gen()
            from action in DomainGen.Action<object>()
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Property.ForAll<object>(gen, action);

                var sample = property.SampleOne(seed: seed);

                sample.Arity.Should().Be(1);
            });

        [Property]
        public NebulaCheck.IGen<Test> ABinaryPropertyHasAnArityOfTwo() =>
            from gens in DomainGen.Gen().Two()
            from action in DomainGen.Action<object, object>()
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Property.ForAll(gens.Item1, gens.Item2, action);

                var sample = property.SampleOne(seed: seed);

                sample.Arity.Should().Be(2);
            });

        [Property]
        public NebulaCheck.IGen<Test> ATernaryPropertyHasAnArityOfThree() =>
            from gens in DomainGen.Gen().Three()
            from action in DomainGen.Action<object, object, object>()
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Property.ForAll(gens.Item1, gens.Item2, gens.Item3, action);

                var sample = property.SampleOne(seed: seed);

                sample.Arity.Should().Be(3);
            });

        [Property]
        public NebulaCheck.IGen<Test> AQuarternaryPropertyHasAnArityOfFour() =>
            from gens in DomainGen.Gen().Four()
            from action in DomainGen.Action<object, object, object, object>()
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Property.ForAll(gens.Item1, gens.Item2, gens.Item3, gens.Item4, action);

                var sample = property.SampleOne(seed: seed);

                sample.Arity.Should().Be(4);
            });
    }
}
