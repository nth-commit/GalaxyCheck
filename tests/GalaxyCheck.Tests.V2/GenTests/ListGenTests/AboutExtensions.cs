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
        public NebulaCheck.IGen<Test> BetweenLengthsDefersToUnderlyingMethods() =>
            from minLength in Gen.Int32().LessThanEqual(20)
            from maxLength in Gen.Int32().GreaterThanEqual(minLength)
            select Property.ForThese(() =>
            {
                {
                    var mockGen = SetupMock();

                    mockGen.Object.BetweenLengths(minLength, maxLength);

                    mockGen.Verify(gen => gen.OfMinimumLength(minLength), Times.Once);
                    mockGen.Verify(gen => gen.OfMaximumLength(maxLength), Times.Once);
                }

                {
                    var mockGen = SetupMock();

                    mockGen.Object.BetweenLengths(maxLength, minLength);

                    mockGen.Verify(gen => gen.OfMinimumLength(minLength), Times.Once);
                    mockGen.Verify(gen => gen.OfMaximumLength(maxLength), Times.Once);
                }
            });

        private static Mock<IListGen<object>> SetupMock()
        {
            var mockGen = new Mock<IListGen<object>>();
            mockGen.Setup((gen) => gen.OfMinimumLength(It.IsAny<int>())).Returns(mockGen.Object);
            mockGen.Setup((gen) => gen.OfMaximumLength(It.IsAny<int>())).Returns(mockGen.Object);
            return mockGen;
        }
    }
}
