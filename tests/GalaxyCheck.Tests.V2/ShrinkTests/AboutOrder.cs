using NebulaCheck;
using NebulaCheck.Xunit;
using System;
using System.Linq;
using GalaxyCheck.Internal.ExampleSpaces;
using FluentAssertions;

namespace Tests.V2.ShrinkTests
{
    public class AboutOrder
    {
        [Property]
        public IGen<Test> IfListIsOrdered_ItWillNotShrink() =>
            from keySelector in Gen.Function<int, int>(Gen.Int32()).NoShrink()
            from list in Gen.Int32().ListOf().Select(l => l.OrderBy(keySelector))
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Order(keySelector);

                var shrinks = func(list.ToList());

                shrinks.Should().BeEmpty();
            });

        [Property]
        public IGen<Test> IfListIsNotOrdered_ItWillProduceASingleOrderedShrink() =>
            from keySelector in Gen.Function<int, int>(Gen.Int32()).NoShrink()
            from list in Gen.Int32().ListOf()
            where list.SequenceEqual(list.OrderBy(keySelector)) == false
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Order(keySelector);

                var shrinks = func(list.ToList());

                shrinks.Should()
                    .ContainSingle()
                    .Subject.Should().BeEquivalentTo(list.OrderBy(keySelector));
            });
    }
}
