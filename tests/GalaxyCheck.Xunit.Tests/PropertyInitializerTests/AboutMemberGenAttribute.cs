using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Gens;
using GalaxyCheck.Xunit.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Tests.PropertyInitializerTests
{
    public class AboutMemberGenAttribute
    {

        private static readonly IGen<int> Gen = GalaxyCheck.Gen.Int32();

#pragma warning disable xUnit1000 // Test classes must be public
        private class Properties
#pragma warning restore xUnit1000 // Test classes must be public
        {
            public object NotAGen => new object();

            [Property]
            public void PropertyWithNonGenMemberGen([MemberGen(nameof(NotAGen))] int x)
            {
            }

            public IGen<int> PropertyMemberGen => Gen;

            [Property]
            public void PropertyWithPropertyMemberGen([MemberGen(nameof(PropertyMemberGen))] int x)
            {
            }

            public IGen<int> FieldMemberGen = Gen;

            [Property]
            public void PropertyWithFieldMemberGen([MemberGen(nameof(FieldMemberGen))] int x)
            {
            }

            public IGen<int> PublicMemberGen => Gen;

            [Property]
            public void PropertyWithPublicMemberGen([MemberGen(nameof(PublicMemberGen))] int x)
            {
            }

            private IGen<int> PrivateMemberGen => Gen;

            [Property]
            public void PropertyWithPrivateMemberGen([MemberGen(nameof(PrivateMemberGen))] int x)
            {
            }

            public IGen<int> InstanceMemberGen => Gen;

            [Property]
            public void PropertyWithInstanceMemberGen([MemberGen(nameof(InstanceMemberGen))] int x)
            {
            }

            public static IGen<int> StaticMemberGen => Gen;

            [Property]
            public void PropertyWithStaticMemberGen([MemberGen(nameof(StaticMemberGen))] int x)
            {
            }

            [Property]
            public void PropertyWithMemberGenAtIndex0([MemberGen(nameof(PropertyMemberGen))] int x, int y)
            {
            }

            [Property]
            public void PropertyWithMemberGenAtIndex1(int x, [MemberGen(nameof(PropertyMemberGen))] int y)
            {
            }
        }

        [Theory]
        [InlineData(nameof(Properties.PropertyWithNonGenMemberGen))]
        public void ItErrorsWhenReferencePropertyIsNotAGen(string testMethodName)
        {
            var testClassType = typeof(Properties);
            var testMethodInfo = GetMethod(testMethodName);

            Action test = () => PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { }, new DefaultPropertyFactory());

            test.Should()
                .Throw<Exception>()
                .WithMessage("Expected member 'NotAGen' to be an instance of 'GalaxyCheck.IGen', but it had a value of type 'System.Object'");
        }


        [Theory]
        [InlineData(nameof(Properties.PropertyWithPropertyMemberGen))]
        [InlineData(nameof(Properties.PropertyWithFieldMemberGen))]
        [InlineData(nameof(Properties.PropertyWithPublicMemberGen))]
        [InlineData(nameof(Properties.PropertyWithPrivateMemberGen))]
        [InlineData(nameof(Properties.PropertyWithInstanceMemberGen))]
        [InlineData(nameof(Properties.PropertyWithStaticMemberGen))]
        public void ItPassesThroughGen(string testMethodName)
        {
            var mockPropertyFactory = MockPropertyFactory();
            var testClassType = typeof(Properties);
            var testMethodInfo = GetMethod(testMethodName);

            PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { }, mockPropertyFactory.Object);

            VerifyGenPassedThrough(mockPropertyFactory, Gen, 0);
        }

        [Theory]
        [InlineData(nameof(Properties.PropertyWithMemberGenAtIndex0), 0)]
        [InlineData(nameof(Properties.PropertyWithMemberGenAtIndex1), 1)]
        public void ItPassesThroughGenKeyedToCorrectIndex(string testMethodName, int expectedIndex)
        {
            var mockPropertyFactory = MockPropertyFactory();
            var testClassType = typeof(Properties);
            var testMethodInfo = GetMethod(testMethodName);

            PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { }, mockPropertyFactory.Object);

            VerifyGenPassedThrough(mockPropertyFactory, Gen, expectedIndex);
        }

        private static MethodInfo GetMethod(string name)
        {
            var methodInfo = typeof(Properties).GetMethod(name, BindingFlags.Instance | BindingFlags.Public);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }

        private static Mock<IPropertyFactory> MockPropertyFactory()
        {
            var mock = new Mock<IPropertyFactory>();

            return mock;
        }

        private static void VerifyGenPassedThrough(Mock<IPropertyFactory> mockPropertyFactory, IGen expectedGen, int expectedParameterIndex)
        {
            mockPropertyFactory.Verify(
                x => x.CreateProperty(
                    It.IsAny<MethodInfo>(),
                    It.IsAny<object?>(),
                    It.IsAny<IGenFactory>(),
                    It.Is<IReadOnlyDictionary<int, IGen>>(x => x.SequenceEqual(new [] { new KeyValuePair<int, IGen>(expectedParameterIndex, expectedGen) }))),
                Times.Once);
        }
    }
}
