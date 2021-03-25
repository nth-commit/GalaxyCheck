using GalaxyCheck;
using GalaxyCheck.Gens;
using Moq;
using NebulaCheck;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.StringGenTests
{
    public class AboutExtensions
    {
        [Property]
        public NebulaCheck.IGen<Test> WithLengthBetweenDefersToUnderlyingMethods() =>
            from minLength in Gen.Int32().LessThanEqual(20)
            from maxLength in Gen.Int32().GreaterThanEqual(minLength)
            select Property.ForThese(() =>
            {
                {
                    var mockGen = SetupMock();

                    mockGen.Object.WithLengthBetween(minLength, maxLength);

                    mockGen.Verify(gen => gen.WithLengthGreaterThanEqual(minLength), Times.Once);
                    mockGen.Verify(gen => gen.WithLengthLessThanEqual(maxLength), Times.Once);
                }

                {
                    var mockGen = SetupMock();

                    mockGen.Object.WithLengthBetween(maxLength, minLength);

                    mockGen.Verify(gen => gen.WithLengthGreaterThanEqual(minLength), Times.Once);
                    mockGen.Verify(gen => gen.WithLengthLessThanEqual(maxLength), Times.Once);
                }
            });

        [Property]
        public NebulaCheck.IGen<Test> GreaterThanLengthDefersToUnderlyingMethod() =>
            from minLengthExclusive in Gen.Int32()
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                mockGen.Object.WithLengthGreaterThan(minLengthExclusive);

                mockGen.Verify(gen => gen.WithLengthGreaterThanEqual(minLengthExclusive + 1), Times.Once);
            });

        [Property]
        public NebulaCheck.IGen<Test> LessThanLengthDefersToUnderlyingMethod() =>
            from maxLengthExclusive in Gen.Int32()
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                mockGen.Object.WithLengthLessThan(maxLengthExclusive);

                mockGen.Verify(gen => gen.WithLengthLessThanEqual(maxLengthExclusive - 1), Times.Once);
            });

        private static Mock<IStringGen> SetupMock()
        {
            var mockGen = new Mock<IStringGen>();
            mockGen.Setup((gen) => gen.WithLengthGreaterThanEqual(It.IsAny<int>())).Returns(mockGen.Object);
            mockGen.Setup((gen) => gen.WithLengthLessThanEqual(It.IsAny<int>())).Returns(mockGen.Object);
            return mockGen;
        }
    }
}
