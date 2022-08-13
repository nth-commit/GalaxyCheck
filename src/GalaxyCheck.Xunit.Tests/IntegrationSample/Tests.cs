using GalaxyCheck;
using GalaxyCheck.Gens;
using GalaxyCheck.Gens.Injection.Int32;
using Newtonsoft.Json;
using System;
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
            AnnounceTestInvocation(nameof(InfallibleVoidProperty));
        }

        [Property]
        public void FallibleVoidProperty([Between(0, 100)] int x)
        {
            AnnounceTestInvocation(nameof(FallibleVoidProperty));
            throw new Exception("Failed!");
        }

        [Property]
        public void InfallibleBooleanProperty([Between(0, 100)] int x)
        {
            AnnounceTestInvocation(nameof(InfallibleBooleanProperty));
        }

        [Property]
        public void FallibleBooleanProperty([Between(0, 100)] int x)
        {
            AnnounceTestInvocation(nameof(FallibleBooleanProperty));
            throw new Exception("Failed!");
        }

        [Property]
        public Property InfallibleReturnedProperty()
        {
            AnnounceTestInvocation(nameof(InfallibleReturnedProperty));
            return Gen.Int32().Between(0, 100).ForAll(y => { });
        }

        [Property]
        public Property FallibleReturnedProperty()
        {
            AnnounceTestInvocation(nameof(FallibleReturnedProperty));
            return Gen.Int32().Between(0, 100).ForAll(y => { throw new Exception("Failed!"); });
        }

        public class GenFactoryWhereIntsAreNonNegativeAttribute : GenFactoryAttribute
        {
            public override IGenFactory Value => Gen.Factory().RegisterType(Gen.Int32().GreaterThanEqual(0));
        }

        [Property]
        [GenFactoryWhereIntsAreNonNegative]
        public void PropertyWithGenFromGenFactory(int x)
        {
            AnnounceTestInvocation(nameof(PropertyWithGenFromGenFactory), new [] { x });
            Assert.True(x >= 0, "They are not negative!");
        }

        private static IGen<int> EvenInt32 => Gen.Int32().Where(x => x % 2 == 0);

        [Property]
        public void PropertyWithGenFromMemberGen([MemberGen(nameof(EvenInt32))] int x)
        {
            AnnounceTestInvocation(nameof(PropertyWithGenFromGenFactory), new[] { x });
            Assert.True(x % 2 != 1, "They are not odd!");
        }

        [Sample]
        public void SampleVoidProperty([Between(0, 100)] int x)
        {
        }

        private void AnnounceTestInvocation(string testName, params object[] injectedValues)
        {
            _testOutputHelper.WriteLine(JsonConvert.SerializeObject(new
            {
                testName,
                injectedValues
            }));
        }
    }
}
