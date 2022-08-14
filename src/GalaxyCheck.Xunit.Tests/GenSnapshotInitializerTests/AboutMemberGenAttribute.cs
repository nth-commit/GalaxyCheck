using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Gens;
using GalaxyCheck.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Tests.GenSnapshotInitializerTests
{
    public class AboutMemberGenAttribute
    {

        private static readonly IGen<int> Gen = GalaxyCheck.Gen.Int32();

#pragma warning disable xUnit1000 // Test classes must be public
        private class GenSnapshots
#pragma warning restore xUnit1000 // Test classes must be public
        {
            public object NotAGen => new object();

            [GenSnapshot]
            public object GenSnapshotWithNonGenMemberGen([MemberGen(nameof(NotAGen))] int x) => new { };

            public IGen<int> GenSnapshotMemberGen => Gen;

            [GenSnapshot]
            public object GenSnapshotWithGenSnapshotMemberGen([MemberGen(nameof(GenSnapshotMemberGen))] int x) => new { };

            public IGen<int> FieldMemberGen = Gen;

            [GenSnapshot]
            public object GenSnapshotWithFieldMemberGen([MemberGen(nameof(FieldMemberGen))] int x) => new { };

            public IGen<int> PublicMemberGen => Gen;

            [GenSnapshot]
            public object GenSnapshotWithPublicMemberGen([MemberGen(nameof(PublicMemberGen))] int x) => new { };

            private IGen<int> PrivateMemberGen => Gen;

            [GenSnapshot]
            public object GenSnapshotWithPrivateMemberGen([MemberGen(nameof(PrivateMemberGen))] int x) => new { };

            public IGen<int> InstanceMemberGen => Gen;

            [GenSnapshot]
            public object GenSnapshotWithInstanceMemberGen([MemberGen(nameof(InstanceMemberGen))] int x) => new { };

            public static IGen<int> StaticMemberGen => Gen;

            [GenSnapshot]
            public object GenSnapshotWithStaticMemberGen([MemberGen(nameof(StaticMemberGen))] int x) => new { };

            [GenSnapshot]
            public object GenSnapshotWithMemberGenAtIndex0([MemberGen(nameof(GenSnapshotMemberGen))] int x, int y) => new { };

            [GenSnapshot]
            public object GenSnapshotWithMemberGenAtIndex1(int x, [MemberGen(nameof(GenSnapshotMemberGen))] int y) => new { };
        }

        [Theory]
        [InlineData(nameof(GenSnapshots.GenSnapshotWithNonGenMemberGen))]
        public void ItErrorsWhenReferenceGenSnapshotIsNotAGen(string testMethodName)
        {
            Action test = () => TestProxy.Initialize(typeof(GenSnapshots), testMethodName);

            test.Should()
                .Throw<Exception>()
                .WithMessage("Expected member 'NotAGen' to be an instance of 'GalaxyCheck.IGen', but it had a value of type 'System.Object'");
        }


        [Theory]
        [InlineData(nameof(GenSnapshots.GenSnapshotWithGenSnapshotMemberGen))]
        [InlineData(nameof(GenSnapshots.GenSnapshotWithFieldMemberGen))]
        [InlineData(nameof(GenSnapshots.GenSnapshotWithPublicMemberGen))]
        [InlineData(nameof(GenSnapshots.GenSnapshotWithPrivateMemberGen))]
        [InlineData(nameof(GenSnapshots.GenSnapshotWithInstanceMemberGen))]
        [InlineData(nameof(GenSnapshots.GenSnapshotWithStaticMemberGen))]
        public void ItPassesThroughGen(string testMethodName)
        {
            var mockGenSnapshotFactory = MockGenSnapshotFactory();

            TestProxy.Initialize(typeof(GenSnapshots), testMethodName, parametersGenFactory: mockGenSnapshotFactory.Object);

            VerifyGenPassedThrough(mockGenSnapshotFactory, Gen, 0);
        }

        [Theory]
        [InlineData(nameof(GenSnapshots.GenSnapshotWithMemberGenAtIndex0), 0)]
        [InlineData(nameof(GenSnapshots.GenSnapshotWithMemberGenAtIndex1), 1)]
        public void ItPassesThroughGenKeyedToCorrectIndex(string testMethodName, int expectedIndex)
        {
            var mockGenSnapshotFactory = MockGenSnapshotFactory();

            TestProxy.Initialize(typeof(GenSnapshots), testMethodName, parametersGenFactory: mockGenSnapshotFactory.Object);

            VerifyGenPassedThrough(mockGenSnapshotFactory, Gen, expectedIndex);
        }

        private static MethodInfo GetMethod(string name)
        {
            var methodInfo = typeof(GenSnapshots).GetMethod(name, BindingFlags.Instance | BindingFlags.Public);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }

        private static Mock<IParametersGenFactory> MockGenSnapshotFactory()
        {
            var mock = new Mock<IParametersGenFactory>();

            return mock;
        }

        private static void VerifyGenPassedThrough(Mock<IParametersGenFactory> mockGenSnapshotFactory, IGen expectedGen, int expectedParameterIndex)
        {
            mockGenSnapshotFactory.Verify(
                x => x.CreateParametersGen(
                    It.IsAny<MethodInfo>(),
                    It.IsAny<IGenFactory>(),
                    It.Is<IReadOnlyDictionary<int, IGen>>(x => x.SequenceEqual(new [] { new KeyValuePair<int, IGen>(expectedParameterIndex, expectedGen) }))),
                Times.Once);
        }
    }
}
