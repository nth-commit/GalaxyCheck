using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Gens;
using Moq;
using NebulaCheck;
using System;
using System.Linq;
using Xunit;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.Int32GenTests
{
    public class AboutExtensions
    {
        [Property]
        public NebulaCheck.IGen<Test> BetweenDefersToUnderlyingMethods() =>
            from min in Gen.Int32()
            from max in Gen.Int32().GreaterThanEqual(min)
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
            from minExclusive in Gen.Int32().LessThan(int.MaxValue)
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

                var gen = mockGen.Object.GreaterThan(int.MaxValue);

                mockGen.Verify(gen0 => gen0.GreaterThanEqual(It.IsAny<int>()), Times.Never);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithGenErrorMessage("Arithmetic operation resulted in an overflow.");
            });

        [Property]
        public NebulaCheck.IGen<Test> LessThanDefersToUnderlyingMethod() =>
            from maxExclusive in Gen.Int32().GreaterThan(int.MinValue)
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

                var gen = mockGen.Object.LessThan(int.MinValue);

                mockGen.Verify(gen => gen.LessThanEqual(It.IsAny<int>()), Times.Never);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithGenErrorMessage("Arithmetic operation resulted in an overflow.");
            });

        private static Mock<IIntGen<int>> SetupMock()
        {
            var mockGen = new Mock<IIntGen<int>>();
            mockGen.Setup((gen) => gen.GreaterThanEqual(It.IsAny<int>())).Returns(mockGen.Object);
            mockGen.Setup((gen) => gen.LessThanEqual(It.IsAny<int>())).Returns(mockGen.Object);
            return mockGen;
        }
    }
}
