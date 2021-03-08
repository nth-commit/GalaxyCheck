using FluentAssertions;
using GalaxyCheck;
using System.Linq;
using Xunit;

namespace Tests.V2.PropertyTests.LinqInferenceTests
{
    public class AboutTransformationWhenLinqInferenceIsDisabled
    {
        private static Property<object> ToProperty(IGen<Test> gen) =>
            new Property(gen, new PropertyOptions { EnableLinqInference = false });

        [Fact]
        public void ItDoesNotPopulateThePresentationalValue()
        {
            var property = ToProperty(
                from a in Gen.Int32()
                select Property.ForThese(() => false));

            var result = property.Check(seed: 0);

            var counterexample = result.Counterexample!;
            var expectedValue = Property.ForThese(() => { }).Input;
            result.Counterexample!.Value.Should().BeEquivalentTo(expectedValue);
            result.Counterexample!.PresentationalValue.Should().BeNull();
        }

        [Fact]
        public void ItAllowsAnonymousObjectsToBeExplicitlyReturnedBySelect()
        {
            var property = ToProperty(
                from obj in Gen.Int32().Select(a => new { a })
                select Property.ForThese(() => true));

            var result = property.Check(seed: 0);

            result.Counterexample.Should().BeNull();
        }

        [Fact]
        public void ItAllowsAnonymousObjectsToBeExplicitlyReturnedBySelectMany()
        {
            var property = ToProperty(
                from obj in Gen.Int32().Select(a => Gen.Constant(new { a }))
                select Property.ForThese(() => true));

            var result = property.Check(seed: 0);

            result.Counterexample.Should().BeNull();
        }

        [Fact]
        public void ItAllowsAnonymousObjectsToBeExplicitlyReturnedBySelectManyWithProjection()
        {
            var property = ToProperty(
                from obj in Gen.Int32().SelectMany(a => Gen.Constant(new { a }), (x, y) => new { x, y })
                select Property.ForThese(() => true));

            var result = property.Check(seed: 0);

            result.Counterexample.Should().BeNull();
        }
    }
}
