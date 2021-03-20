using FluentAssertions;
using GalaxyCheck.ExampleSpaces;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Linq;

namespace Tests.V2.ShrinkTests
{
    public class AboutBisect
    {
        [Property]
        public IGen<Test> IfListIsEmpty_ItWillNotShrink() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().OfCount(0)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Bisect<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });

        [Property]
        public IGen<Test> IfListHasOneElement_ItWillNotShrink() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().OfCount(1)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Bisect<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });

        [Property]
        public IGen<Test> IfListLengthIsEqualToMinLength_ItWillNotShrink() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().OfCount(minLength)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Bisect<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });

        [Property]
        public IGen<Test> IfListLengthIsLessThanDoubleMinLength_ItWillNotShrink() =>
            from minLength in TestGen.MinLength(minMinLength: 1)
            from list in DomainGen.AnyList().WithCountLessThan(minLength * 2)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Bisect<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });

        [Property]
        public IGen<Test> ItWillProduceTwoShrinksOfSimilarLengths() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().WithCountGreaterThanEqual((minLength + 1) * 2)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Bisect<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().HaveCount(2);
                shrinks.First().Count.Should().BeCloseTo(shrinks.Skip(1).Single().Count, 1);
            });

        [Property]
        public IGen<Test> ItWillNotProduceShrinksLessThanMinimumLength() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().WithCountGreaterThanEqual((minLength + 1) * 2)
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
            from list in DomainGen.AnyList().WithCountGreaterThanEqual((minLength + 1) * 2)
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
        }
    }
}
