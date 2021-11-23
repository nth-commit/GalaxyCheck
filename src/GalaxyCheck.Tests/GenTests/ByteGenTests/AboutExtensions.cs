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

namespace Tests.V2.GenTests.ByteGenTests
{
    public class AboutExtensions
    {
        [Property]
        public NebulaCheck.IGen<Test> BetweenDefersToUnderlyingMethods() =>
            from min in Gen.Byte()
            from max in Gen.Byte().GreaterThanEqual(min)
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
            from minExclusive in Gen.Byte().LessThan(byte.MaxValue)
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                mockGen.Object.GreaterThan(minExclusive);

                mockGen.Verify(gen => gen.GreaterThanEqual((byte)(minExclusive + 1)), Times.Once);
            });

        [Property]
        public NebulaCheck.IGen<Test> GreaterThanErrorsAtMaxValue() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                var gen = mockGen.Object.GreaterThan(byte.MaxValue);

                mockGen.Verify(gen => gen.GreaterThanEqual(It.IsAny<byte>()), Times.Never);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator Byte().GreaterThan(255): Arithmetic operation resulted in an overflow.");
            });

        [Property]
        public NebulaCheck.IGen<Test> LessThanDefersToUnderlyingMethod() =>
            from maxExclusive in Gen.Byte().GreaterThan(byte.MinValue)
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                mockGen.Object.LessThan(maxExclusive);

                mockGen.Verify(gen => gen.LessThanEqual((byte)(maxExclusive - 1)), Times.Once);
            });

        [Property]
        public NebulaCheck.IGen<Test> LessThanErrorsAtMinValue() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                var gen = mockGen.Object.LessThan(byte.MinValue);

                mockGen.Verify(gen => gen.LessThanEqual(It.IsAny<byte>()), Times.Never);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator Byte().LessThan(0): Arithmetic operation resulted in an overflow.");
            });

        private static Mock<IIntGen<byte>> SetupMock()
        {
            var mockGen = new Mock<IIntGen<byte>>();
            mockGen.Setup((gen) => gen.GreaterThanEqual(It.IsAny<byte>())).Returns(mockGen.Object);
            mockGen.Setup((gen) => gen.LessThanEqual(It.IsAny<byte>())).Returns(mockGen.Object);
            return mockGen;
        }
    }
}
