using GalaxyCheck;
using GalaxyCheck.Gens;
using GalaxyCheck.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Tests.PropertyInitializerTests
{
    public class AboutFactory
    {
        [Theory]
        [InlineData(nameof(PropertiesWithoutFactoryConfig.PropertyWithoutFactoryConfig))]
        [InlineData(nameof(PropertiesWithoutFactoryConfig.SampleWithoutFactoryConfig))]
        public void WhenClassNorMethodHasFactoryConfig_ItPassesThroughNull(string testMethodName)
        {
            var mockPropertyFactory = MockPropertyFactory();
            var testClassType = typeof(PropertiesWithoutFactoryConfig);
            var testMethodInfo = GetMethod(testClassType, testMethodName);

            PropertyInitializer.Initialize(
                testClassType,
                testMethodInfo,
                new object[] { },
                mockPropertyFactory.Object,
                new GlobalPropertyConfiguration());

            VerifyFactoryNotPassedThrough(mockPropertyFactory);
        }

        [Theory]
        [InlineData(nameof(PropertiesWithoutFactoryConfig.PropertyWithFactoryConfig))]
        [InlineData(nameof(PropertiesWithoutFactoryConfig.SampleWithFactoryConfig))]
        public void ItPassesThroughTheFactoryFromTheMethod(string testMethodName)
        {
            var mockPropertyFactory = MockPropertyFactory();
            var testClassType = typeof(PropertiesWithoutFactoryConfig);
            var testMethodInfo = GetMethod(testClassType, testMethodName);

            PropertyInitializer.Initialize(
                testClassType,
                testMethodInfo,
                new object[] { },
                mockPropertyFactory.Object,
                new GlobalPropertyConfiguration());

            VerifyFactoryPassedThrough(mockPropertyFactory, typeof(GenFactory1));
        }

        [Theory]
        [InlineData(nameof(PropertiesWithFactoryConfig.PropertyWithoutFactoryConfig))]
        [InlineData(nameof(PropertiesWithFactoryConfig.SampleWithoutFactoryConfig))]
        public void ItPassesThroughTheFactoryFromTheClass(string testMethodName)
        {
            var mockPropertyFactory = MockPropertyFactory();
            var testClassType = typeof(PropertiesWithFactoryConfig);
            var testMethodInfo = GetMethod(testClassType, testMethodName);

            PropertyInitializer.Initialize(
                testClassType,
                testMethodInfo,
                new object[] { },
                mockPropertyFactory.Object,
                new GlobalPropertyConfiguration());

            VerifyFactoryPassedThrough(mockPropertyFactory, typeof(GenFactory2));
        }

        [Theory]
        [InlineData(nameof(PropertiesWithFactoryConfig.PropertyWithFactoryConfig))]
        [InlineData(nameof(PropertiesWithFactoryConfig.SampleWithFactoryConfig))]
        public void AMethodLevelFactoryIsPreferredOverAClassLevelFactory(string testMethodName)
        {
            var mockPropertyFactory = MockPropertyFactory();
            var testClassType = typeof(PropertiesWithFactoryConfig);
            var testMethodInfo = GetMethod(testClassType, testMethodName);

            PropertyInitializer.Initialize(
                testClassType,
                testMethodInfo,
                new object[] { },
                mockPropertyFactory.Object,
                new GlobalPropertyConfiguration());

            VerifyFactoryPassedThrough(mockPropertyFactory, typeof(GenFactory1));
        }

        private static Mock<IPropertyFactory> MockPropertyFactory()
        {
            var mock = new Mock<IPropertyFactory>();

            return mock;
        }

        private static void VerifyFactoryPassedThrough(Mock<IPropertyFactory> mockPropertyFactory, Type expectedFactoryType)
        {
            mockPropertyFactory.Verify(
                x => x.CreateProperty(
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?>(),
                    It.Is<IGenFactory?>(factory => factory!.GetType() == expectedFactoryType),
                    It.IsAny<IReadOnlyDictionary<int, IGen>>()),
                Times.Once);
        }

        private static void VerifyFactoryNotPassedThrough(Mock<IPropertyFactory> mockPropertyFactory)
        {
            mockPropertyFactory.Verify(
                x => x.CreateProperty(
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?>(),
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
        private class PropertiesWithoutFactoryConfig
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [Property]
            [GenFactory1]
            public void PropertyWithFactoryConfig()
            {
            }

            [Sample]
            [GenFactory1]
            public void SampleWithFactoryConfig()
            {
            }

            [Property]
            public void PropertyWithoutFactoryConfig()
            {
            }

            [Sample]
            public void SampleWithoutFactoryConfig()
            {
            }
        }

        [GenFactory2]
#pragma warning disable xUnit1000 // Test classes must be public
        private class PropertiesWithFactoryConfig
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [Property]
            [GenFactory1]
            public void PropertyWithFactoryConfig()
            {
            }

            [Sample]
            [GenFactory1]
            public void SampleWithFactoryConfig()
            {
            }

            [Property]
            public void PropertyWithoutFactoryConfig()
            {
            }

            [Sample]
            public void SampleWithoutFactoryConfig()
            {
            }
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
