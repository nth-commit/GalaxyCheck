using GalaxyCheck;
using GalaxyCheck.Gens;
using GalaxyCheck.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Tests.GenSnapshotInitializerTests
{
    public class AboutFactory
    {
        [Theory]
        [InlineData(nameof(GenSnapshotsWithoutFactoryConfig.GenSnapshotWithoutFactoryConfig))]
        public void WhenClassNorMethodHasFactoryConfig_ItPassesThroughNull(string testMethodName)
        {
            var mockParametersGenFactory = MockParametersGenFactory();

            TestProxy.Initialize(
                typeof(GenSnapshotsWithoutFactoryConfig),
                testMethodName,
                parametersGenFactory: mockParametersGenFactory.Object);

            VerifyFactoryNotPassedThrough(mockParametersGenFactory);
        }

        [Theory]
        [InlineData(nameof(GenSnapshotsWithoutFactoryConfig.GenSnapshotWithFactoryConfig))]
        public void ItPassesThroughTheFactoryFromTheMethod(string testMethodName)
        {
            var mockParametersGenFactory = MockParametersGenFactory();

            TestProxy.Initialize(
                typeof(GenSnapshotsWithoutFactoryConfig),
                testMethodName,
                parametersGenFactory: mockParametersGenFactory.Object);

            VerifyFactoryPassedThrough(mockParametersGenFactory, typeof(GenFactory1));
        }

        [Theory]
        [InlineData(nameof(GenSnapshotsWithFactoryConfig.GenSnapshotWithoutFactoryConfig))]
        public void ItPassesThroughTheFactoryFromTheClass(string testMethodName)
        {
            var mockParametersGenFactory = MockParametersGenFactory();

            TestProxy.Initialize(
                typeof(GenSnapshotsWithFactoryConfig),
                testMethodName,
                parametersGenFactory: mockParametersGenFactory.Object);

            VerifyFactoryPassedThrough(mockParametersGenFactory, typeof(GenFactory2));
        }

        [Theory]
        [InlineData(nameof(GenSnapshotsWithFactoryConfig.GenSnapshotWithFactoryConfig))]
        public void AMethodLevelFactoryIsPreferredOverAClassLevelFactory(string testMethodName)
        {
            var mockParametersGenFactory = MockParametersGenFactory();

            TestProxy.Initialize(
                typeof(GenSnapshotsWithFactoryConfig),
                testMethodName,
                parametersGenFactory: mockParametersGenFactory.Object);

            VerifyFactoryPassedThrough(mockParametersGenFactory, typeof(GenFactory1));
        }

        private static Mock<IParametersGenFactory> MockParametersGenFactory()
        {
            var mock = new Mock<IParametersGenFactory>();

            return mock;
        }

        private static void VerifyFactoryPassedThrough(Mock<IParametersGenFactory> mockParametersGenFactory, Type expectedFactoryType)
        {
            mockParametersGenFactory.Verify(
                x => x.CreateParametersGen(
                    It.IsAny<MethodInfo>(),
                    It.Is<IGenFactory?>(factory => factory != null && factory.GetType() == expectedFactoryType),
                    It.IsAny<IReadOnlyDictionary<int, IGen>>()),
                Times.Once);
        }

        private static void VerifyFactoryNotPassedThrough(Mock<IParametersGenFactory> mockParametersGenFactory)
        {
            mockParametersGenFactory.Verify(
                x => x.CreateParametersGen(
                    It.IsAny<MethodInfo>(),
                    It.Is<IGenFactory?>(x => x == null),
                    It.IsAny<IReadOnlyDictionary<int, IGen>>()),
                Times.Once);
        }

        private class GenFactory1 : IGenFactory
        {
            public IReflectedGen<T> Create<T>(NullabilityInfo? nullabilityInfo = null) where T : notnull
            {
                throw new NotImplementedException();
            }

            public IGenFactory RegisterType(Type type, IGen gen)
            {
                throw new NotImplementedException();
            }

            public IGenFactory RegisterType<T>(IGen<T> gen)
            {
                throw new NotImplementedException();
            }

            public IGenFactory RegisterType<T>(Func<IGenFactory, IGen<T>> genFunc)
            {
                throw new NotImplementedException();
            }

            public IGenFactory RegisterType(Type type, Func<IGenFactory, IGen> genFunc)
            {
                throw new NotImplementedException();
            }
        }

        private class GenFactory1Attribute : GenFactoryAttribute
        {
            public override IGenFactory Value => new GenFactory1();
        }

        private class GenFactory2 : IGenFactory
        {
            public IReflectedGen<T> Create<T>(NullabilityInfo? nullabilityInfo = null) where T : notnull
            {
                throw new NotImplementedException();
            }

            public IGenFactory RegisterType(Type type, IGen gen)
            {
                throw new NotImplementedException();
            }

            public IGenFactory RegisterType<T>(IGen<T> gen)
            {
                throw new NotImplementedException();
            }

            public IGenFactory RegisterType<T>(Func<IGenFactory, IGen<T>> genFunc)
            {
                throw new NotImplementedException();
            }

            public IGenFactory RegisterType(Type type, Func<IGenFactory, IGen> genFunc)
            {
                throw new NotImplementedException();
            }
        }

        private class GenFactory2Attribute : GenFactoryAttribute
        {
            public override IGenFactory Value => new GenFactory2();
        }

#pragma warning disable xUnit1000 // Test classes must be public
        private class GenSnapshotsWithoutFactoryConfig
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [GenSnapshot]
            [GenFactory1]
            public object GenSnapshotWithFactoryConfig() => new { };

            [GenSnapshot]
            public object GenSnapshotWithoutFactoryConfig() => new { };
        }

        [GenFactory2]
#pragma warning disable xUnit1000 // Test classes must be public
        private class GenSnapshotsWithFactoryConfig
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [GenSnapshot]
            [GenFactory1]
            public object GenSnapshotWithFactoryConfig() => new { };

            [GenSnapshot]
            public object GenSnapshotWithoutFactoryConfig() => new { };
        }


        private static MethodInfo GetMethod(Type type, string name)
        {
            var methodInfo = type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }
    }
}
