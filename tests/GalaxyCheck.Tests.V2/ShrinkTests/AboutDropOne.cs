using FluentAssertions;
using GalaxyCheck.Internal.ExampleSpaces;
using NebulaCheck;
using NebulaCheck.Xunit;
using System;
using System.Linq;

namespace Tests.V2.ShrinkTests
{
    public class AboutDropOne
    {
        [Property]
        public IGen<Test> IfListIsEmpty_ItWillNotShrink() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().OfLength(0)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.DropOne<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });

        [Property]
        public IGen<Test> IfListLengthIsLessThanOrEqualMinLength_ItWillNotShrink() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().OfMaximumLength(minLength)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.DropOne<object>(minLength);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });


        [Property]
        public IGen<Test> ItWillNotProduceShrinksLessThanMinimumLength() =>
            from minLength in TestGen.MinLength()
            from list in DomainGen.AnyList().BetweenLengths(minLength + 1, Math.Max(minLength + 10, 25))
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
            from list in DomainGen.AnyList().BetweenLengths(minLength + 1, Math.Max(minLength + 10, 25))
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
            from list in DomainGen.AnyList().BetweenLengths(minLength + 1, Math.Max(minLength + 10, 25))
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
