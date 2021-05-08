using GalaxyCheck;
using GalaxyCheck.Gens;
using Moq;
using NebulaCheck;
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
            from minExclusive in Gen.Byte()
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                mockGen.Object.GreaterThan(minExclusive);

                mockGen.Verify(gen => gen.GreaterThanEqual((byte)(minExclusive + 1)), Times.Once);
            });

        [Property]
        public NebulaCheck.IGen<Test> LessThanDefersToUnderlyingMethod() =>
            from maxExclusive in Gen.Byte()
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                mockGen.Object.LessThan(maxExclusive);

                mockGen.Verify(gen => gen.LessThanEqual((byte)(maxExclusive - 1)), Times.Once);
            });

        private static Mock<IIntegerGen<byte>> SetupMock()
        {
            var mockGen = new Mock<IIntegerGen<byte>>();
            mockGen.Setup((gen) => gen.GreaterThanEqual(It.IsAny<byte>())).Returns(mockGen.Object);
            mockGen.Setup((gen) => gen.LessThanEqual(It.IsAny<byte>())).Returns(mockGen.Object);
            return mockGen;
        }
    }
}
