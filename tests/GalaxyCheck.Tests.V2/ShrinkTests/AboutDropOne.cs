using FluentAssertions;
using GalaxyCheck.ExampleSpaces;
using NebulaCheck;
using System;
using System.Linq;

namespace Tests.V2.ShrinkTests
{
    public class AboutDropOne
    {
        [Property]
        public IGen<Test> IfListIsEmpty_ItWillNotShrink() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().OfCount(0)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.DropOne<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });

        [Property]
        public IGen<Test> IfListLengthIsLessThanOrEqualMinLength_ItWillNotShrink() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().WithCountLessThanEqual(minLength)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.DropOne<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });


        [Property]
        public IGen<Test> ItWillNotProduceShrinksLessThanMinimumLength() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().WithCountGreaterThan(minLength)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.DropOne<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should()
                    .NotBeEmpty().And
                    .OnlyContain(shrink => shrink.Count >= minLength);
            });

        [Property]
        public IGen<Test> ItWillProduceAShrinkForEachElementInTheList() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().WithCountGreaterThan(minLength)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.DropOne<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should()
                    .NotBeEmpty().And
                    .OnlyContain(shrink => shrink.Count == list.Count - 1);
            });

        [Property]
        public IGen<Test> ItWillProduceDistinctShrinks() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().WithCountGreaterThan(minLength)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.DropOne<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().OnlyHaveUniqueItems();
            });

        private static class TestGen
        {
            public static IGen<int> MinLength(int? minMinLength = null) =>
                Gen.Int32().Between(minMinLength ?? 0, 25);
        }
    }
}
