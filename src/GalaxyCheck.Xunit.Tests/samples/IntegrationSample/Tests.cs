using GalaxyCheck;
using GalaxyCheck.Gens;
using GalaxyCheck.Gens.Injection.Int32;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationSample
{
    public class Tests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public Tests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Property]
        public void InfallibleVoidProperty([Between(0, 100)] int x)
        {
            AnnounceTestInvocation(new object[] { x });
        }

        [Property]
        public void FallibleVoidProperty([Between(0, 100)] int x)
        {
            AnnounceTestInvocation(new object[] { x });
            throw new Exception("Failed!");
        }

        [Property]
        public void InfallibleBooleanProperty([Between(0, 100)] int x)
        {
            AnnounceTestInvocation(new object[] { x });
        }

        [Property]
        public void FallibleBooleanProperty([Between(0, 100)] int x)
        {
            AnnounceTestInvocation(new object[] { x });
            throw new Exception("Failed!");
        }

        [Property]
        public Property InfallibleReturnedProperty()
        {
            return Gen.Int32().Between(0, 100).ForAll(x => { AnnounceTestInvocation(new object[] { x }); });
        }

        [Property]
        public Property FallibleReturnedProperty()
        {
            return Gen.Int32().Between(0, 100).ForAll(x =>
            {
                AnnounceTestInvocation(new object[] { x });
                throw new Exception("Failed!");
            });
        }

        public class GenFactoryWhereIntsAreNonNegativeAttribute : GenFactoryAttribute
        {
            public override IGenFactory Value => Gen.Factory().RegisterType(Gen.Int32().GreaterThanEqual(0));
        }

        [Property]
        [GenFactoryWhereIntsAreNonNegative]
        public void PropertyWithGenFromGenFactory(int x)
        {
            AnnounceTestInvocation(new object[] { x });
            Assert.True(x >= 0, "They are not negative!");
        }

        private static IGen<int> EvenInt32 => Gen.Int32().Where(x => x % 2 == 0);

        [Property]
        public void PropertyWithGenFromMemberGen([MemberGen(nameof(EvenInt32))] int x)
        {
            AnnounceTestInvocation(new object[] { x });
            Assert.True(x % 2 != 1, "They are not odd!");
        }

        [Property]
        [InlineData(0)]
        [InlineData(1)]
        public Property InfalliblePropertyWithControlData(int x)
        {
            return Gen.Int32().Between(0, 100).ForAll(y => { AnnounceTestInvocation(new object[] { x, y }, new object[] { x }); });
        }

        [Property]
        [InlineData(0)]
        [InlineData(1)]
        public Property FalliblePropertyWithControlData(int x)
        {
            return Gen.Int32().Between(0, 100).ForAll(y =>
            {
                AnnounceTestInvocation(new object[] { x, y }, new object[] { x });
                return x != 1;
            });
        }

        [Sample]
        public void SampleVoidProperty([Between(0, 100)] int x)
        {
        }

        private void AnnounceTestInvocation(object[] injectedValues, object[]? controlData = null, [CallerMemberName] string testName = "")
        {
            if (controlData is not null)
            {
                testName += $"_{string.Join("_", controlData)}";
            }

            _testOutputHelper.WriteLine(JsonConvert.SerializeObject(new
            {
                testName,
                injectedValues
            }));
        }
    }
}
