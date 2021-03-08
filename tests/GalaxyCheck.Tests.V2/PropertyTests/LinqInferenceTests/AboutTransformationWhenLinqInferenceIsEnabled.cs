using FluentAssertions;
using GalaxyCheck;
using System;
using System.Linq;
using Xunit;

namespace Tests.V2.PropertyTests.LinqInferenceTests
{
    public class AboutTransformationWhenLinqInferenceIsEnabled
    {
        private static Property<object> ToProperty(IGen<Test> gen) =>
            new Property(gen, new PropertyOptions { EnableLinqInference = true });

        [Fact]
        public void ItPopulatesThePresentationalValueWithAScopedVariable()
        {
            var value = 0;

            var property = ToProperty(
                from a in Gen.Constant(value)
                select Property.ForThese(() => false));

            var result = property.Check();

            result.Counterexample!.PresentationalValue
                .Should().BeEquivalentTo(value);
        }

        [Fact]
        public void ItPopulatesThePresentationalValueWithTwoScopedVariables()
        {
            var value0 = 0;
            var value1 = 1;

            var property = ToProperty(
                from a in Gen.Constant(value0)
                from b in Gen.Constant(value1)
                select Property.ForThese(() => false));

            var result = property.Check();

            result.Counterexample!.PresentationalValue
                .Should().BeEquivalentTo(new [] { value0, value1 });
        }

        [Fact]
        public void ItPopulatesThePresentationalValueWithThreeScopedVariables()
        {
            var value0 = 0;
            var value1 = 1;
            var value2 = 2;

            var property = ToProperty(
                from a in Gen.Constant(value0)
                from b in Gen.Constant(value1)
                from c in Gen.Constant(value2)
                select Property.ForThese(() => false));

            var result = property.Check();

            result.Counterexample!.PresentationalValue
                .Should().BeEquivalentTo(new[] { value0, value1, value2 });
        }

        [Fact]
        public void ItPopulatesThePresentationalValueWithAScopedVariableAndIgnoresInnerSelect()
        {
            var value_ignored = 0;
            var value = 1;

            var property = ToProperty(
                from a in Gen.Constant(value_ignored).Select(_ => value)
                select Property.ForThese(() => false));

            var result = property.Check();

            result.Counterexample!.PresentationalValue
                .Should().BeEquivalentTo(value);
        }

        [Fact]
        public void ItPopulatesThePresentationalValueWithAScopedVariableAndIgnoresInnerSelectMany()
        {
            var value_ignored = 0;
            var value = 1;

            var property = ToProperty(
                from a in Gen.Constant(value_ignored).SelectMany(_ => Gen.Constant(value))
                select Property.ForThese(() => false));

            var result = property.Check();

            result.Counterexample!.PresentationalValue
                .Should().BeEquivalentTo(value);
        }

        [Fact]
        public void ItErrorsWhenInnerSelectExplicitlyReturnsAnAnonymousType()
        {
            var value_ignored = 0;
            var value = new { a = 0, b = 1 };

            var property = ToProperty(
                from a in Gen.Constant(value_ignored).Select(_ => value)
                select Property.ForThese(() => false));

            Action action = () => property.Check();

            action.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator Select: Anonymous types are forbidden");
        }

        [Fact]
        public void ItErrorsWhenInnerSelectManyExplicitlyReturnsAnAnonymousType()
        {
            var value_ignored = 0;
            var value = new { a = 0, b = 1 };

            var property = ToProperty(
                from a in Gen.Constant(value_ignored).SelectMany(_ => Gen.Constant(value))
                select Property.ForThese(() => false));

            Action action = () => property.Check();

            action.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator SelectMany: Anonymous types are forbidden");
        }
    }
}
