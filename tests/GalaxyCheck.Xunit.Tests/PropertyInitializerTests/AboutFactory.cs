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
        [InlineData(nameof(PropertiesWithoutFactoryConfig.PropertyWithFactoryConfigThatIsNotAnAutoGenFactory))]
        [InlineData(nameof(PropertiesWithoutFactoryConfig.SampleWithFactoryConfigThatIsNotAnAutoGenFactory))]
        public void WhenFactoryTypeIsNotAnIAutoGenFactory_ItThrows(string testMethodName)
        {
            var testClassType = typeof(PropertiesWithoutFactoryConfig);
            var testMethodInfo = GetMethod(testMethodName);

            Action test = () => PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { }, new DefaultPropertyFactory());

            test.Should()
                .Throw<PropertyConfigurationException>()
                .WithMessage("Factory must implement 'GalaxyCheck.Gens.IAutoGenFactory' but 'Tests.PropertyInitializerTests.AboutFactory+NotAnAutoGenFactory' did not.");
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
                .WithMessage("Factory must have a default constructor, but 'Tests.PropertyInitializerTests.AboutFactory+AutoGenFactoryWithoutDefaultConstructor' did not.");
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

            VerifyFactoryPassedThrough(mockPropertyFactory, typeof(ValidAutoGenFactory1));
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

            VerifyFactoryPassedThrough(mockPropertyFactory, typeof(ValidAutoGenFactory2));
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

            VerifyFactoryPassedThrough(mockPropertyFactory, typeof(ValidAutoGenFactory1));
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
                    It.Is<IAutoGenFactory?>(factory => factory!.GetType() == expectedFactoryType)),
                Times.Once);
        }

        private static void VerifyFactoryNotPassedThrough(Mock<IPropertyFactory> mockPropertyFactory)
        {
            mockPropertyFactory.Verify(
                x => x.CreateProperty(
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?>(),
                    It.Is<IAutoGenFactory?>(x => x == null)),
                Times.Once);
        }

        private class NotAnAutoGenFactory { }

        private class AutoGenFactoryWithoutDefaultConstructor : IAutoGenFactory
        {
            private AutoGenFactoryWithoutDefaultConstructor() { }

            public IAutoGen<T> Create<T>()
            {
                throw new NotImplementedException();
            }

            public IAutoGenFactory RegisterType(Type type, IGen gen)
            {
                throw new NotImplementedException();
            }
        }

        private class ValidAutoGenFactory1 : IAutoGenFactory
        {
            public IAutoGen<T> Create<T>()
            {
                throw new NotImplementedException();
            }

            public IAutoGenFactory RegisterType(Type type, IGen gen)
            {
                throw new NotImplementedException();
            }
        }

        private class ValidAutoGenFactory2 : IAutoGenFactory
        {
            public IAutoGen<T> Create<T>()
            {
                throw new NotImplementedException();
            }

            public IAutoGenFactory RegisterType(Type type, IGen gen)
            {
                throw new NotImplementedException();
            }
        }

#pragma warning disable xUnit1000 // Test classes must be public
        private class PropertiesWithoutFactoryConfig
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [Property(Factory = typeof(NotAnAutoGenFactory))]
            public void PropertyWithFactoryConfigThatIsNotAnAutoGenFactory()
            {
            }

            [Sample(Factory = typeof(NotAnAutoGenFactory))]
            public void SampleWithFactoryConfigThatIsNotAnAutoGenFactory()
            {
            }

            [Property(Factory = typeof(AutoGenFactoryWithoutDefaultConstructor))]
            public void PropertyWithFactoryConfigThatDoesNotHaveDefaultConstructor()
            {
            }

            [Sample(Factory = typeof(AutoGenFactoryWithoutDefaultConstructor))]
            public void SampleWithFactoryConfigThatThatDoesNotHaveDefaultConstructor()
            {
            }

            [Property(Factory = typeof(ValidAutoGenFactory1))]
            public void PropertyWithFactoryConfig()
            {
            }

            [Sample(Factory = typeof(ValidAutoGenFactory1))]
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

        [Properties(Factory = typeof(ValidAutoGenFactory2))]
#pragma warning disable xUnit1000 // Test classes must be public
        private class PropertiesWithFactoryConfig
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [Property(Factory = typeof(ValidAutoGenFactory1))]
            public void PropertyWithFactoryConfig()
            {
            }

            [Sample(Factory = typeof(ValidAutoGenFactory1))]
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
