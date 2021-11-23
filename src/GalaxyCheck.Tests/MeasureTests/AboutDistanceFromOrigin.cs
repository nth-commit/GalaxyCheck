using FluentAssertions;
using GalaxyCheck.ExampleSpaces;
using NebulaCheck;
using System;
using System.Linq;
using Xunit;

namespace Tests.V2.MeasureTests
{
    public class AboutDistanceFromOrigin
    {
        [Property]
        public IGen<Test> IfMinIsGreaterThanMax_ItThrows() =>
            from min in Gen.Int32().GreaterThan(int.MinValue)
            from max in Gen.Int32().LessThan(min)
            from origin in Gen.Int32()
            select Property.ForThese(() =>
            {
                Action action = () => MeasureFunc.DistanceFromOrigin(origin, min, max);

                action.Should()
                    .Throw<ArgumentOutOfRangeException>()
                    .And.ParamName.Should().Be("min");
            });

        [Property]
        public IGen<Test> IfOriginIsLessThanMin_ItThrows() =>
            from min in Gen.Int32().GreaterThan(int.MinValue)
            from max in Gen.Int32().GreaterThanEqual(min)
            from origin in Gen.Int32().LessThan(min)
            select Property.ForThese(() =>
            {
                Action action = () => MeasureFunc.DistanceFromOrigin(origin, min, max);

                action.Should()
                    .Throw<ArgumentOutOfRangeException>()
                    .And.ParamName.Should().Be("origin");
            });

        [Property]
        public IGen<Test> IfOriginIsGreaterThanMax_ItThrows() =>
            from min in Gen.Int32().LessThan(int.MaxValue)
            from max in Gen.Int32().GreaterThanEqual(min).LessThan(int.MaxValue)
            from origin in Gen.Int32().GreaterThan(max)
            select Property.ForThese(() =>
            {
                Action action = () => MeasureFunc.DistanceFromOrigin(origin, min, max);

                action.Should()
                    .Throw<ArgumentOutOfRangeException>()
                    .And.ParamName.Should().Be("origin");
            });

        [Property]
        public IGen<Test> IfValueIsLessThanMin_ItThrows() =>
            from min in Gen.Int32().GreaterThan(int.MinValue)
            from max in Gen.Int32().GreaterThanEqual(min)
            from origin in Gen.Int32().Between(min, max)
            from value in Gen.Int32().LessThan(min)
            select Property.ForThese(() =>
            {
                var func = MeasureFunc.DistanceFromOrigin(origin, min, max);

                Action action = () => func(value);

                action.Should()
                    .Throw<ArgumentOutOfRangeException>()
                    .And.ParamName.Should().Be("value");
            });

        [Property]
        public IGen<Test> IfValueIsGreaterThanMax_ItThrows() =>
            from min in Gen.Int32().LessThan(int.MaxValue)
            from max in Gen.Int32().GreaterThanEqual(min).LessThan(int.MaxValue)
            from origin in Gen.Int32().Between(min, max)
            from value in Gen.Int32().GreaterThan(max)
            select Property.ForThese(() =>
            {
                var func = MeasureFunc.DistanceFromOrigin(origin, min, max);

                Action action = () => func(value);

                action.Should()
                    .Throw<ArgumentOutOfRangeException>()
                    .And.ParamName.Should().Be("value");
            });

        [Property]
        public IGen<Test> IfValueEqualsOrigin_ItReturnsZero() =>
            from min in Gen.Int32()
            from max in Gen.Int32().GreaterThanEqual(min)
            from origin in Gen.Int32().Between(min, max)
            let value = origin
            select Property.ForThese(() =>
            {
                var func = MeasureFunc.DistanceFromOrigin(origin, min, max);

                var distance = func(value);

                distance.Should().Be(0);
            });

        [Property]
        public IGen<Test> IfValueEqualsMin_AndOriginDiffers_ItReturns100() =>
            from min in Gen.Int32()
            from max in Gen.Int32().GreaterThanEqual(min)
            from origin in Gen.Int32().Between(min, max)
            where origin != min
            let value = min
            select Property.ForThese(() =>
            {
                var func = MeasureFunc.DistanceFromOrigin(origin, min, max);

                var distance = func(value);

                distance.Should().Be(100);
            });

        [Property]
        public IGen<Test> IfValueEqualsMax_AndOriginDiffers_ItReturns100() =>
            from min in Gen.Int32()
            from max in Gen.Int32().GreaterThanEqual(min)
            from origin in Gen.Int32().Between(min, max)
            where origin != max
            let value = max
            select Property.ForThese(() =>
            {
                var func = MeasureFunc.DistanceFromOrigin(origin, min, max);

                var distance = func(value);

                distance.Should().Be(100);
            });

        [Theory]
        [InlineData(0, -100, 50, 25, 50)]
        [InlineData(50, -50, 100, 75, 50)]
        [InlineData(0, 0, 4, 1, 25)]
        [InlineData(0, 0, 3, 1, 33.33333)]
        public void ExamplesForValuesBetweenOriginAndMax(int origin, int min, int max, int value, decimal expectedDistance)
        {
            var func = MeasureFunc.DistanceFromOrigin(origin, min, max);

            var distance = func(value);

            distance.Should().BeApproximately(expectedDistance, 1);
        }
    }
}
