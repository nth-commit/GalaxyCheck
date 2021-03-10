using GalaxyCheck;
using GalaxyCheck.Gens;
using Moq;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ListGenTests
{
    public class AboutExtensions
    {
        [Property]
        public NebulaCheck.IGen<Test> BetweenCountsDefersToUnderlyingMethods() =>
            from minCount in Gen.Int32().LessThanEqual(20)
            from maxCount in Gen.Int32().GreaterThanEqual(minCount)
            select Property.ForThese(() =>
            {
                {
                    var mockGen = SetupMock();

                    mockGen.Object.BetweenCounts(minCount, maxCount);

                    mockGen.Verify(gen => gen.WithCountGreaterThanEqual(minCount), Times.Once);
                    mockGen.Verify(gen => gen.WithCountLessThanEqual(maxCount), Times.Once);
                }

                {
                    var mockGen = SetupMock();

                    mockGen.Object.BetweenCounts(maxCount, minCount);

                    mockGen.Verify(gen => gen.WithCountGreaterThanEqual(minCount), Times.Once);
                    mockGen.Verify(gen => gen.WithCountLessThanEqual(maxCount), Times.Once);
                }
            });

        private static Mock<IListGen<object>> SetupMock()
        {
            var mockGen = new Mock<IListGen<object>>();
            mockGen.Setup((gen) => gen.WithCountGreaterThanEqual(It.IsAny<int>())).Returns(mockGen.Object);
            mockGen.Setup((gen) => gen.WithCountLessThanEqual(It.IsAny<int>())).Returns(mockGen.Object);
            return mockGen;
        }
    }
}
