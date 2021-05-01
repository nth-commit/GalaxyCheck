using GalaxyCheck;
using GalaxyCheck.Gens;
using Moq;
using System;
using Xunit;

namespace Tests.V2.GenTests.AutoGenTests
{
    public class AboutAutoGenFactoryExtensions
    {
        [Fact]
        public void GenericRegisterTypeDefersToNonGenericRegisterType()
        {
            var gen = Gen.Int32();
            var type = typeof(int);

            var mockFactory = new Mock<IAutoGenFactory>();
            mockFactory
                .Setup(factory => factory.RegisterType(
                    It.IsAny<Type>(),
                    It.IsAny<IGen>()))
                .Returns(mockFactory.Object);

            mockFactory.Object.RegisterType(gen);

            mockFactory.Verify(
                factory => factory.RegisterType(
                    It.Is<Type>(t => t == type),
                    It.Is<IGen>(g => g == gen)),
                Times.Once);
        }
    }
}
