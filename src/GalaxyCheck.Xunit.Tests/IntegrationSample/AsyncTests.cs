using GalaxyCheck;
using GalaxyCheck.Gens;
using GalaxyCheck.Gens.Injection.Int32;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationSample
{
    public class AsyncTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public AsyncTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Property]
        public async Task InfallibleVoidPropertyAsync([Between(0, 100)] int x)
        {
            await Task.Delay(1);
            AnnounceTestInvocation(nameof(InfallibleVoidPropertyAsync));
        }

        [Property]
        public async Task FallibleVoidPropertyAsync([Between(0, 100)] int x)
        {
            await Task.Delay(1);
            AnnounceTestInvocation(nameof(FallibleVoidPropertyAsync));
            throw new Exception("Failed!");
        }

        [Property]
        public async Task InfallibleBooleanPropertyAsync([Between(0, 100)] int x)
        {
            await Task.Delay(1);
            AnnounceTestInvocation(nameof(InfallibleBooleanPropertyAsync));
        }

        [Property]
        public async Task FallibleBooleanPropertyAsync([Between(0, 100)] int x)
        {
            await Task.Delay(1);
            AnnounceTestInvocation(nameof(FallibleBooleanPropertyAsync));
            throw new Exception("Failed!");
        }

        [Property]
        public AsyncProperty InfallibleReturnedPropertyAsync()
        {
            AnnounceTestInvocation(nameof(InfallibleReturnedPropertyAsync));
            return Gen.Int32().Between(0, 100).ForAllAsync(async y =>
            { 
                await Task.Delay(1);
            });
        }

        [Property]
        public AsyncProperty FallibleReturnedPropertyAsync()
        {
            AnnounceTestInvocation(nameof(FallibleReturnedPropertyAsync));
            return Gen.Int32().Between(0, 100).ForAllAsync(async y =>
            {
                await Task.Delay(1);
                throw new Exception("Failed!");
            });
        }

        public class GenFactoryWhereIntsAreNonNegativeAttribute : GenFactoryAttribute
        {
            public override IGenFactory Value => Gen.Factory().RegisterType(Gen.Int32().GreaterThanEqual(0));
        }

        [Property]
        [GenFactoryWhereIntsAreNonNegative]
        public async Task PropertyWithGenFromGenFactoryAsync(int x)
        {
            await Task.Delay(1);
            AnnounceTestInvocation(nameof(PropertyWithGenFromGenFactoryAsync), new [] { x });
            Assert.True(x >= 0, "They are not negative!");
        }

        private static IGen<int> EvenInt32 => Gen.Int32().Where(x => x % 2 == 0);

        [Property]
        public async Task PropertyWithGenFromMemberGenAsync([MemberGen(nameof(EvenInt32))] int x)
        {
            await Task.Delay(1);
            AnnounceTestInvocation(nameof(PropertyWithGenFromGenFactoryAsync), new[] { x });
            Assert.True(x % 2 != 1, "They are not odd!");
        }

        [Sample]
        public async Task SampleVoidPropertyAsync([Between(0, 100)] int x)
        {
            await Task.Delay(1);
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
