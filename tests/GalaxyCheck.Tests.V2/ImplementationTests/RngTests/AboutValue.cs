using FluentAssertions;
using NebulaCheck;
using NebulaCheck.Xunit;
using System;

namespace Tests.V2.ImplementationTests.RngTests
{
    public class AboutValue
    {
        [Property]
        public IGen<Test> ItIsPure() =>
            from rng in RandomDomainGen.Rng()
            from min in Gen.Int32()
            from max in Gen.Int32().GreaterThanEqual(min)
            select Property.ForThese(() =>
            {
                rng.Value(min, max).Should().Be(rng.Value(min, max));
            });

        [Property]
        public IGen<Test> IfMinimumIsEqualToMaximum_ItReturnsTheValue() =>
            from rng in RandomDomainGen.Rng()
            from value in Gen.Int32()
            select Property.ForThese(() =>
            {
                rng.Value(value, value).Should().Be(value);
            });

        [Property]
        public IGen<Test> IfMinimumIsGreaterThanMaximum_ItThrows() =>
            from rng in RandomDomainGen.Rng()
            from min in Gen.Int32().GreaterThan(int.MinValue)
            from max in Gen.Int32().LessThan(min)
            select Property.ForThese(() =>
            {
                Action test = () => rng.Value(min, max);

                test.Should().Throw<ArgumentOutOfRangeException>();
            });
    }
}
