using GalaxyCheck;
using GalaxyCheck.Gens;
using Moq;
using NebulaCheck;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.SetGenTests
{
    public class AboutExtensions
    {
        [Property]
        public NebulaCheck.IGen<Test> WithCountBetweenDefersToUnderlyingMethods() =>
            from minCount in Gen.Int32().LessThanEqual(20)
            from maxCount in Gen.Int32().GreaterThanEqual(minCount)
            select Property.ForThese(() =>
            {
                {
                    var mockGen = SetupMock();

                    mockGen.Object.WithCountBetween(minCount, maxCount);

                    mockGen.Verify(gen => gen.WithCountGreaterThanEqual(minCount), Times.Once);
                    mockGen.Verify(gen => gen.WithCountLessThanEqual(maxCount), Times.Once);
                }

                {
                    var mockGen = SetupMock();

                    mockGen.Object.WithCountBetween(maxCount, minCount);

                    mockGen.Verify(gen => gen.WithCountGreaterThanEqual(minCount), Times.Once);
                    mockGen.Verify(gen => gen.WithCountLessThanEqual(maxCount), Times.Once);
                }
            });

        [Property]
        public NebulaCheck.IGen<Test> GreaterThanCountDefersToUnderlyingMethod() =>
            from minCountExclusive in Gen.Int32()
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                mockGen.Object.WithCountGreaterThan(minCountExclusive);

                mockGen.Verify(gen => gen.WithCountGreaterThanEqual(minCountExclusive + 1), Times.Once);
            });

        [Property]
        public NebulaCheck.IGen<Test> LessThanCountDefersToUnderlyingMethod() =>
            from maxCountExclusive in Gen.Int32()
            select Property.ForThese(() =>
            {
                var mockGen = SetupMock();

                mockGen.Object.WithCountLessThan(maxCountExclusive);

                mockGen.Verify(gen => gen.WithCountLessThanEqual(maxCountExclusive - 1), Times.Once);
            });

        private static Mock<ISetGen<object>> SetupMock()
        {
            var mockGen = new Mock<ISetGen<object>>();
            mockGen.Setup((gen) => gen.WithCountGreaterThanEqual(It.IsAny<int>())).Returns(mockGen.Object);
            mockGen.Setup((gen) => gen.WithCountLessThanEqual(It.IsAny<int>())).Returns(mockGen.Object);
            return mockGen;
        }
    }
}
