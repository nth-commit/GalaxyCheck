using FluentAssertions;
using GalaxyCheck.Internal.WeightedLists;
using NebulaCheck;
using NebulaCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.V2.WeightedListTests
{
    public class AboutWeightedList
    {
        public static TheoryData<List<WeightedElement<string>>, List<string>> Data =>
            new TheoryData<List<WeightedElement<string>>, List<string>>
            {
                {
                    new List<WeightedElement<string>>
                    {
                        new WeightedElement<string>(0, "a")
                    },
                    new List<string> { }
                },
                {
                    new List<WeightedElement<string>>
                    {
                        new WeightedElement<string>(1, "a"),
                        new WeightedElement<string>(0, "b")
                    },
                    new List<string> { "a" }
                },
                {
                    new List<WeightedElement<string>>
                    {
                        new WeightedElement<string>(2, "a"),
                    },
                    new List<string> { "a", "a" }
                },
                {
                    new List<WeightedElement<string>>
                    {
                        new WeightedElement<string>(3, "a"),
                        new WeightedElement<string>(2, "b")
                    },
                    new List<string> { "a", "a", "a", "b", "b" }
                },
            };

        [Theory]
        [MemberData(nameof(Data))]
        public void Examples(List<WeightedElement<string>> weightedElements, List<string> expectedValues)
        {
            var weightedList = new WeightedList<string>(weightedElements);

            weightedList.Should().BeEquivalentTo(expectedValues);
        }

        [Property]
        public IGen<Test> IfAWeightIsNegative_ItThrows() =>
            from weight in Gen.Int32().LessThan(0)
            from value in DomainGen.Any()
            select Property.ForThese(() =>
            {
                var source = new[] { new WeightedElement<object>(weight, value) };

                Action action = () => new WeightedList<object>(source);

                action.Should()
                    .Throw<ArgumentException>()
                    .WithMessage("Weighted element must be positive (Parameter 'weightedElements')")
                    .And.ParamName.Should().Be("weightedElements");
            });

        [Property]
        public IGen<Test> IfTheIndexIsNegative_ItThrows() =>
            from source in TestGen.WeightedElement().ListOf()
            from index in Gen.Int32().LessThan(0)
            select Property.ForThese(() =>
            {
                var list = new WeightedList<object>(source);

                Action action = () =>
                {
                    var _ = list[index];
                };

                action.Should()
                    .Throw<ArgumentException>()
                    .WithMessage("Index must be non-negative (Parameter 'index')")
                    .And.ParamName.Should().Be("index");
            });

        [Property]
        public IGen<Test> IfTheIndexIsGreaterThanOrEqualTheCount_ItThrows() =>
            from source in TestGen.WeightedElement().ListOf()
            from index in Gen.Int32().GreaterThanEqual(source.Count)
            select Property.ForThese(() =>
            {
                var list = new WeightedList<object>(source);

                Action action = () =>
                {
                    var _ = list[index];
                };

                action.Should()
                    .Throw<ArgumentException>()
                    .WithMessage("Index must be less than the total weight of the elements (Parameter 'index')")
                    .And.ParamName.Should().Be("index");
            });

        [Property]
        public IGen<Test> ItReturnsTheFirstValueAtIndexZero() =>
            from first in TestGen.WeightedElement()
            where first.Weight > 0
            from rest in TestGen.WeightedElement().ListOf()
            select Property.ForThese(() =>
            {
                var list = new WeightedList<object>(rest.Prepend(first));

                var value = list[0];

                value.Should().Be(first.Element);
            });

        [Property]
        public IGen<Test> ItReturnsTheFirstValueAtIndexOfTheFirstWeightMinusOne() =>
            from first in TestGen.WeightedElement()
            where first.Weight > 0
            from rest in TestGen.WeightedElement().ListOf()
            select Property.ForThese(() =>
            {
                var list = new WeightedList<object>(rest.Prepend(first));

                var value = list[first.Weight - 1];

                value.Should().Be(first.Element);
            });

        [Property]
        public IGen<Test> ItReturnsTheSecondValueAtTheIndexOfTheFirstWeight() =>
            from first in TestGen.WeightedElement()
            from second in TestGen.WeightedElement()
            where second.Weight > 0
            from rest in TestGen.WeightedElement().ListOf()
            select Property.ForThese(() =>
            {
                var list = new WeightedList<object>(Enumerable.Concat(new[] { first, second }, rest));

                var value = list[first.Weight];

                value.Should().Be(second.Element);
            });

        [Property]
        public IGen<Test> ItReturnsTheLastValueAtTheMaxIndex() =>
            from rest in TestGen.WeightedElement().ListOf()
            from last in TestGen.WeightedElement()
            where last.Weight > 0
            select Property.ForThese(() =>
            {
                var list = new WeightedList<object>(rest.Append(last));

                var value = list[list.Count - 1];

                value.Should().Be(last.Element);
            });

        [Property]
        public IGen<Test> IfAllWeightsAreOne_ItIsEquivalentToSource() =>
            from values in DomainGen.AnyList().OfMinimumLength(1)
            select Property.ForThese(() =>
            {
                var list = new WeightedList<object>(values.Select(value => new WeightedElement<object>(1, value)));

                list.Should().BeEquivalentTo(values);
            });

        private static class TestGen
        {
            public static IGen<WeightedElement<object>> WeightedElement() =>
                from weight in Gen.Int32().Between(0, 100)
                from value in DomainGen.Any()
                select new WeightedElement<object>(weight, value);
        }
    }
}
