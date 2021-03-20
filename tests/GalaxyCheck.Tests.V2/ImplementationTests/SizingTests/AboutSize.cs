using FluentAssertions;
using GalaxyCheck.Gens.Parameters;
using NebulaCheck;
using System;
using System.Linq;

namespace Tests.V2.ImplementationTests.SizingTests
{
    public class AboutSize
    {
        [Property]
        public IGen<Test> ItContainsTheGivenValue() =>
            from value in Gen.Int32().Between(0, 100)
            select Property.ForThese(() =>
            {
                var size = new Size(value);

                size.Value.Should().Be(value);
            });

        [Property]
        public IGen<Test> IfValueIsNegative_ItThrows() =>
            from value in Gen.Int32().LessThan(0)
            select Property.ForThese(() =>
            {
                Action action = () => new Size(value);

                action.Should()
                    .Throw<ArgumentOutOfRangeException>()
                    .WithMessage("'value' must be between 0 and 100 (Parameter 'value')");
            });

        [Property]
        public IGen<Test> IfValueIsGreaterThanOneHundred_ItThrows() =>
            from value in Gen.Int32().GreaterThan(100)
            select Property.ForThese(() =>
            {
                Action action = () => new Size(value);

                action.Should()
                    .Throw<ArgumentOutOfRangeException>()
                    .WithMessage("'value' must be between 0 and 100 (Parameter 'value')");
            });
    }
}
