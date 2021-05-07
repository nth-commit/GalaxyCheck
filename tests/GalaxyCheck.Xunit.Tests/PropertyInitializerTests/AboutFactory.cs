using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Gens;
using GalaxyCheck.Xunit;
using GalaxyCheck.Xunit.Internal;
using Moq;
using System;
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
            var testMethodInfo = GetMethod(testMethodName);

            PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { }, mockPropertyFactory.Object);

            VerifyFactoryNotPassedThrough(mockPropertyFactory);
        }

        [Theory]
        [InlineData(nameof(PropertiesWithoutFactoryConfig.PropertyWithFactoryConfigThatIsNotAnGenFactory))]
        [InlineData(nameof(PropertiesWithoutFactoryConfig.SampleWithFactoryConfigThatIsNotAnGenFactory))]
        public void WhenFactoryTypeIsNotAnIGenFactory_ItThrows(string testMethodName)
        {
            var testClassType = typeof(PropertiesWithoutFactoryConfig);
            var testMethodInfo = GetMethod(testMethodName);

            Action test = () => PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { }, new DefaultPropertyFactory());

            test.Should()
                .Throw<PropertyConfigurationException>()
                .WithMessage("Factory must implement 'GalaxyCheck.Gens.IGenFactory' but 'Tests.PropertyInitializerTests.AboutFactory+NotAnGenFactory' did not.");
        }

        [Theory]
        [InlineData(nameof(PropertiesWithoutFactoryConfig.PropertyWithFactoryConfigThatDoesNotHaveDefaultConstructor))]
        [InlineData(nameof(PropertiesWithoutFactoryConfig.SampleWithFactoryConfigThatThatDoesNotHaveDefaultConstructor))]
        public void WhenFactoryTypeDoesNotHaveDefaultConstructor_ItThrows(string testMethodName)
        {
            var testClassType = typeof(PropertiesWithoutFactoryConfig);
            var testMethodInfo = GetMethod(testMethodName);

            Action test = () => PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { }, new DefaultPropertyFactory());

            test.Should()
                .Throw<PropertyConfigurationException>()
                .WithMessage("Factory must have a default constructor, but 'Tests.PropertyInitializerTests.AboutFactory+GenFactoryWithoutDefaultConstructor' did not.");
        }

        [Theory]
        [InlineData(nameof(PropertiesWithoutFactoryConfig.PropertyWithFactoryConfig))]
        [InlineData(nameof(PropertiesWithoutFactoryConfig.SampleWithFactoryConfig))]
        public void ItPassesThroughTheFactoryFromTheMethod(string testMethodName)
        {
            var mockPropertyFactory = MockPropertyFactory();
            var testClassType = typeof(PropertiesWithoutFactoryConfig);
            var testMethodInfo = GetMethod(testMethodName);

            PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { }, mockPropertyFactory.Object);

            VerifyFactoryPassedThrough(mockPropertyFactory, typeof(ValidGenFactory1));
        }

        [Theory]
        [InlineData(nameof(PropertiesWithFactoryConfig.PropertyWithoutFactoryConfig))]
        [InlineData(nameof(PropertiesWithFactoryConfig.SampleWithoutFactoryConfig))]
        public void ItPassesThroughTheFactoryFromTheClass(string testMethodName)
        {
            var mockPropertyFactory = MockPropertyFactory();
            var testClassType = typeof(PropertiesWithFactoryConfig);
            var testMethodInfo = GetMethod(testMethodName);

            PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { }, mockPropertyFactory.Object);

            VerifyFactoryPassedThrough(mockPropertyFactory, typeof(ValidGenFactory2));
        }

        [Theory]
        [InlineData(nameof(PropertiesWithFactoryConfig.PropertyWithFactoryConfig))]
        [InlineData(nameof(PropertiesWithFactoryConfig.SampleWithFactoryConfig))]
        public void AMethodLevelFactoryIsPreferredOverAClassLevelFactory(string testMethodName)
        {
            var mockPropertyFactory = MockPropertyFactory();
            var testClassType = typeof(PropertiesWithFactoryConfig);
            var testMethodInfo = GetMethod(testMethodName);

            PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { }, mockPropertyFactory.Object);

            VerifyFactoryPassedThrough(mockPropertyFactory, typeof(ValidGenFactory1));
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
                    It.Is<IGenFactory?>(factory => factory!.GetType() == expectedFactoryType)),
                Times.Once);
        }

        private static void VerifyFactoryNotPassedThrough(Mock<IPropertyFactory> mockPropertyFactory)
        {
            mockPropertyFactory.Verify(
                x => x.CreateProperty(
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?>(),
                    It.Is<IGenFactory?>(x => x == null)),
                Times.Once);
        }

        private class NotAnGenFactory { }

        private class GenFactoryWithoutDefaultConstructor : IGenFactory
        {
            private GenFactoryWithoutDefaultConstructor() { }

            public IReflectedGen<T> Create<T>()
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
        }

        private class ValidGenFactory1 : IGenFactory
        {
            public IReflectedGen<T> Create<T>()
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
        }

        private class ValidGenFactory2 : IGenFactory
        {
            public IReflectedGen<T> Create<T>()
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
        }

#pragma warning disable xUnit1000 // Test classes must be public
        private class PropertiesWithoutFactoryConfig
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [Property(Factory = typeof(NotAnGenFactory))]
            public void PropertyWithFactoryConfigThatIsNotAnGenFactory()
            {
            }

            [Sample(Factory = typeof(NotAnGenFactory))]
            public void SampleWithFactoryConfigThatIsNotAnGenFactory()
            {
            }

            [Property(Factory = typeof(GenFactoryWithoutDefaultConstructor))]
            public void PropertyWithFactoryConfigThatDoesNotHaveDefaultConstructor()
            {
            }

            [Sample(Factory = typeof(GenFactoryWithoutDefaultConstructor))]
            public void SampleWithFactoryConfigThatThatDoesNotHaveDefaultConstructor()
            {
            }

            [Property(Factory = typeof(ValidGenFactory1))]
            public void PropertyWithFactoryConfig()
            {
            }

            [Sample(Factory = typeof(ValidGenFactory1))]
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

        [Properties(Factory = typeof(ValidGenFactory2))]
#pragma warning disable xUnit1000 // Test classes must be public
        private class PropertiesWithFactoryConfig
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [Property(Factory = typeof(ValidGenFactory1))]
            public void PropertyWithFactoryConfig()
            {
            }

            [Sample(Factory = typeof(ValidGenFactory1))]
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


        private static MethodInfo GetMethod(string name)
        {
            var methodInfo = typeof(PropertiesWithoutFactoryConfig).GetMethod(name, BindingFlags.Public | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }
    }
}
