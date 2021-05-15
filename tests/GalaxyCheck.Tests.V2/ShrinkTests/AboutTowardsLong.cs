using FluentAssertions;
using GalaxyCheck.ExampleSpaces;
using NebulaCheck;
using System.Linq;
using Xunit;

namespace Tests.V2.ShrinkTests
{
    public class AboutTowardsLong
    {
        [Theory]
        [InlineData(10, 0, new long[] { 0, 5, 8, 9 })]
        [InlineData(0, 10, new long[] { 10, 5, 2, 1 })]
        [InlineData(100, 0, new long[] { 0, 50, 75, 88, 94, 97, 99 })]
        public void Examples(long value, long target, long[] expectedShrinks)
        {
            var func = ShrinkFunc.Towards(target);

            var shrinks = func(value);

            shrinks.Should().BeEquivalentTo(expectedShrinks);
        }

        [Property]
        public IGen<Test> IfValueEqualsTarget_ItWillNotShrink() =>
            from value in Gen.Int32().Select(x => (long)x)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Towards(value);

                var shrinks = func(value);

                shrinks.Should().BeEmpty();
            });

        [Property]
        public IGen<Test> ItWillProduceAShrinkEquallingTheTargetFirst() =>
            from value in Gen.Int32().Select(x => (long)x)
            from target in Gen.Int32().Select(x => (long)x)
            where value != target
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Towards(target);

                var shrinks = func(value);

                shrinks.First().Should().Be(target);
            });

        [Property]
        public IGen<Test> ItWillProduceShrinksBetweenTheValueAndTarget() =>
            from value in Gen.Int32().Select(x => (long)x)
            from target in Gen.Int32().Select(x => (long)x)
            where value < target
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Towards(target);

                var shrinks = func(value);

                shrinks.Should().NotBeEmpty();
                shrinks.ToList().ForEach(shrink => shrink.Should().BeInRange(value, target));
            });

        [Property]
        public IGen<Test> ItWillProduceDistinctShrinks() =>
            from value in Gen.Int32().Select(x => (long)x)
            from target in Gen.Int32().Select(x => (long)x)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Towards(target);

                var shrinks = func(value);

                shrinks.Should().OnlyHaveUniqueItems();
            });

        [Property]
        public IGen<Test> ItWillNotProduceAShrinkEquallingTheValue() =>
            from value in Gen.Int32().Select(x => (long)x)
            from target in Gen.Int32().Select(x => (long)x)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Towards(target);

                var shrinks = func(value);

                shrinks.Should().NotContain(value);
            });

        [Property]
        public IGen<Test> ItReflectsAroundZero() =>
            from value in Gen.Int32().GreaterThanEqual(0).Select(x => (long)x)
            select Property.ForThese(() =>
            {
                var func = ShrinkFunc.Towards(0L);

                var shrinks = func(value);
                var shrinksReflected = func(-value).Select(shrink => -shrink);

                shrinks.Should().BeEquivalentTo(shrinksReflected);
            });
    }
}
