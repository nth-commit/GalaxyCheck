using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Gens;
using Moq;
using NebulaCheck;
using System;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.Int64GenTests
{
    public class AboutExtensions
    {
        [Property]
        public NebulaCheck.IGen<Test> BetweenDefersToUnderlyingMethods() =>
            from min in Gen.Int64()
            from max in Gen.Int64().GreaterThanEqual(min)
            select Property.ForThese(() =>
            {
                {
                    var mockGen = SetupMock();

                    mockGen.Object.Between(min, max);

                    mockGen.Verify(gen => gen.GreaterThanEqual(min), Times.Once);
                    mockGen.Verify(gen => gen.LessThanEqual(max), Times.Once);
                }

                {
                    var mockGen = SetupMock();

                    mockGen.Object.Between(max, min);

                    mockGen.Verify(gen => gen.GreaterThanEqual(min), Times.Once);
                    mockGen.Verify(gen => gen.LessThanEqual(max), Times.Once);
                }
            });

        [Property]
        public NebulaCheck.IGen<Test> GreaterThanDefersToUnderlyingMethod() =>
            from minExclusive in Gen.Int64().LessThan(long.MaxValue)
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                mockGen.Object.GreaterThan(minExclusive);

                mockGen.Verify(gen => gen.GreaterThanEqual(minExclusive + 1), Times.Once);
            });

        [Property]
        public NebulaCheck.IGen<Test> GreaterThanErrorsAtMaxValue() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                var gen = mockGen.Object.GreaterThan(long.MaxValue);

                mockGen.Verify(gen => gen.GreaterThanEqual(It.IsAny<long>()), Times.Never);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator Int64().GreaterThan(9223372036854775807): Arithmetic operation resulted in an overflow.");
            });

        [Property]
        public NebulaCheck.IGen<Test> LessThanDefersToUnderlyingMethod() =>
            from maxExclusive in Gen.Int64().GreaterThan(long.MinValue)
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                mockGen.Object.LessThan(maxExclusive);

                mockGen.Verify(gen => gen.LessThanEqual(maxExclusive - 1), Times.Once);
            });

        [Property]
        public NebulaCheck.IGen<Test> LessThanErrorsAtMinValue() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                var gen = mockGen.Object.LessThan(long.MinValue);

                mockGen.Verify(gen => gen.LessThanEqual(It.IsAny<long>()), Times.Never);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator Int64().LessThan(-9223372036854775808): Arithmetic operation resulted in an overflow.");
            });

        private static Mock<IIntGen<long>> SetupMock()
        {
            var mockGen = new Mock<IIntGen<long>>();
            mockGen.Setup((gen) => gen.GreaterThanEqual(It.IsAny<long>())).Returns(mockGen.Object);
            mockGen.Setup((gen) => gen.LessThanEqual(It.IsAny<long>())).Returns(mockGen.Object);
            return mockGen;
        }
    }
}
