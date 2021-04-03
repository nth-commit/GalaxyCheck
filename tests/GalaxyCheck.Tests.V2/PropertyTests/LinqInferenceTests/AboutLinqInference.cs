using FluentAssertions;
using GalaxyCheck;
using System.Linq;
using Xunit;

namespace Tests.V2.PropertyTests.LinqInferenceTests
{
    public class AboutLinqInference
    {
        private static Property<object> ToProperty(IGen<Test> gen) => new Property(gen);

        [Fact]
        public void ItPopulatesThePresentationalValueWithAScopedVariable()
        {
            var value = 0;

            var property = ToProperty(
                from a in Gen.Constant(value)
                select Property.ForThese(() => false));

            var result = property.Check();

            result.Counterexample!.PresentationalValue.Should().BeEquivalentTo(value);
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

            result.Counterexample!.PresentationalValue.Should().BeEquivalentTo(new [] { value0, value1 });
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

            result.Counterexample!.PresentationalValue.Should().BeEquivalentTo(new[] { value0, value1, value2 });
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

            result.Counterexample!.PresentationalValue.Should().BeEquivalentTo(value);
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

            result.Counterexample!.PresentationalValue.Should().BeEquivalentTo(value);
        }

        [Fact]
        public void ItPopulatesThePresentationalValueWithAScopedVariableFromTheLetKeyword()
        {
            var value0 = 0;
            var value1 = 1;

            var property = ToProperty(
                from a in Gen.Constant(value0)
                let b = a
                let c = value1
                select Property.ForThese(() => false));

            var result = property.Check();

            result.Counterexample!.PresentationalValue.Should().BeEquivalentTo(new[] { value0, value0, value1 });
        }

        /// <summary>
        /// The LINQ inference works by unwrapping anonymous types, to reverse engineer what LINQ expressions do. This
        /// means if you explicitly return anonymous types inside a LINQ statement, those values will get erroneously
        /// unrolled. This is not ideal, but it's not possible (or really tricky + hacky) to disambiguate at the
        /// moment.
        /// </summary>
        public class AboutSuboptimalRendering
        {
            [Fact]
            public void ItAllowsAnonymousObjectsToBeExplicitlyReturnedBySelect()
            {
                // Ideally: { a = value }
                // Actual:  [value]

                var value = 0;

                var property = ToProperty(
                    from obj in Gen.Constant(value).Select(a => new { a })
                    select Property.ForThese(() => false));

                var result = property.Check(seed: 0);

                result.Counterexample!.PresentationalValue.Should().BeEquivalentTo(new[] { value });
            }

            [Fact]
            public void ItAllowsAnonymousObjectsToBeExplicitlyReturnedBySelectMany()
            {
                // Ideally: { a = value }
                // Actual:  [value]

                var value = 0;

                var property = ToProperty(
                    from obj in Gen.Constant(value).SelectMany(a => Gen.Constant(new { a }))
                    select Property.ForThese(() => false));

                var result = property.Check(seed: 0);

                result.Counterexample!.PresentationalValue.Should().BeEquivalentTo(new [] { value });
            }

            [Fact]
            public void ItAllowsAnonymousObjectsToBeExplicitlyReturnedBySelectManyWithProjection()
            {
                // Ideally: { a = value, b = value }
                // Actual:  [value, value]

                var value = 0;

                var property = ToProperty(
                    from obj in Gen.Constant(value).SelectMany(a => Gen.Constant(new { a }), (x, y) => new { x, y })
                    select Property.ForThese(() => false));

                var result = property.Check(seed: 0);

                result.Counterexample!.PresentationalValue.Should().BeEquivalentTo(new[] { value, value });
            }
        }
    }
}
