using FluentAssertions;
using FluentAssertions.Execution;
using GalaxyCheck.Internal.ExampleSpaces;
using NebulaCheck;
using NebulaCheck.Xunit;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Tests.V2.ShrinkTests
{
    public class AboutBisect
    {
        [Property]
        public IGen<Test> IfListIsEmpty_ItWillNotShrink() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().OfLength(0)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Bisect<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });

        [Property]
        public IGen<Test> IfListHasOneElement_ItWillNotShrink() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().OfLength(1)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Bisect<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });

        [Property]
        public IGen<Test> IfListLengthIsEqualToMinLength_ItWillNotShrink() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().OfLength(minLength)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Bisect<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });

        [Property]
        public IGen<Test> IfListLengthIsLessThanDoubleMinLength_ItWillNotShrink() =>
            from minLength in TestGen.MinLength(minMinLength: 1)
            from list in DomainGen.AnyList().OfMaximumLength(minLength * 2 - 1)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Bisect<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });

        [Property]
        public IGen<Test> ItWillProduceTwoShrinksOfSimilarLengths() =>
            from minLength in TestGen.MinLength()
            from list in TestGen.ShrinkableList(minLength)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Bisect<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().HaveCount(2);
                var shrink1Count = shrinks.First().Count;
                var shrink2Count = shrinks.Skip(1).Single().Count;
                Math.Abs(shrink1Count - shrink2Count).Should().BeLessOrEqualTo(1);
            });

        [Property]
        public IGen<Test> ItWillNotProduceShrinksLessThanMinimumLength() =>
            from minLength in TestGen.MinLength()
            from list in TestGen.ShrinkableList(minLength)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Bisect<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should()
                    .NotBeEmpty().And
                    .OnlyContain(shrink => shrink.Count >= minLength);
            });

        [Property]
        public IGen<Test> ItWillProducesShrinksThatCanRecreateTheOriginalList() =>
            from minLength in TestGen.MinLength()
            from list in TestGen.ShrinkableList(minLength)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Bisect<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.SelectMany(x => x).Should().BeEquivalentTo(list.ToList());
            });

        private static class TestGen
        {
            public static IGen<int> MinLength(int? minMinLength = null) =>
                Gen.Int32().Between(minMinLength ?? 0, 25);

            public static IGen<ImmutableList<object>> ShrinkableList(int minShrunkLength)
            {
                var minLength = Math.Max(2, minShrunkLength * 2);
                return DomainGen.AnyList().BetweenLengths(minLength, minLength + 10);
            }
        }
    }
}
