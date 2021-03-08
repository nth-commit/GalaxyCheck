using FluentAssertions;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Linq;

namespace Tests.V2.PropertyTests.BooleanPropertyTests
{
    public class AboutArity
    {
        [Property]
        public IGen<Test> ANullaryPropertyHasAnArityOfZero() =>
            from func in Gen.Function(DomainGen.Boolean())
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Property.Nullary(func);

                var sample = GalaxyCheck.Gen.SampleOne(property, seed: seed);

                sample.Arity.Should().Be(0);
            });

        [Property]
        public IGen<Test> AUnaryPropertyHasAnArityOfOne() =>
            from gen in DomainGen.Gen()
            from func in Gen.Function<object, bool>(DomainGen.Boolean())
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Property.ForAll<object>(gen, func);

                var sample = GalaxyCheck.Gen.SampleOne(property, seed: seed);

                sample.Arity.Should().Be(1);
            });

        [Property]
        public IGen<Test> ABinaryPropertyHasAnArityOfTwo() =>
            from gens in DomainGen.Gen().Two()
            from func in Gen.Function<object, object, bool>(DomainGen.Boolean())
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Property.ForAll(gens.Item1, gens.Item2, func);

                var sample = GalaxyCheck.Gen.SampleOne(property, seed: seed);

                sample.Arity.Should().Be(2);
            });

        [Property]
        public IGen<Test> ATernaryPropertyHasAnArityOfThree() =>
            from gens in DomainGen.Gen().Three()
            from func in Gen.Function<object, object, object, bool>(DomainGen.Boolean())
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Property.ForAll(gens.Item1, gens.Item2, gens.Item3, func);

                var sample = GalaxyCheck.Gen.SampleOne(property, seed: seed);

                sample.Arity.Should().Be(3);
            });

        [Property]
        public IGen<Test> AQuarternaryPropertyHasAnArityOfFour() =>
            from gens in DomainGen.Gen().Four()
            from func in Gen.Function<object, object, object, object, bool>(DomainGen.Boolean())
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Property.ForAll(gens.Item1, gens.Item2, gens.Item3, gens.Item4, func);

                var sample = GalaxyCheck.Gen.SampleOne(property, seed: seed);

                sample.Arity.Should().Be(4);
            });

        [Property]
        public IGen<Test> AUnaryPropertyWithAnExplicitArityHasTheGivenArity() =>
            from arity in Gen.Int32().GreaterThanEqual(0)
            from gen in DomainGen.Gen()
            from func in Gen.Function<object, bool>(DomainGen.Boolean())
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var property = GalaxyCheck.Property.ForAll<object>(gen, func, arity);

                var sample = GalaxyCheck.Gen.SampleOne(property, seed: seed);

                sample.Arity.Should().Be(arity);
            });
    }
}
