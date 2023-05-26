using GalaxyCheck;
using GalaxyCheck.Gens;
using GalaxyCheck.Gens.Injection.Int32;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
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
            await Task.CompletedTask;
            AnnounceTestInvocation(new object[] { x });
        }

        [Property]
        public async Task FallibleVoidPropertyAsync([Between(0, 100)] int x)
        {
            await Task.CompletedTask;
            AnnounceTestInvocation(new object[] { x });
            throw new Exception("Failed!");
        }

        [Property]
        public async Task InfallibleBooleanPropertyAsync([Between(0, 100)] int x)
        {
            await Task.CompletedTask;
            AnnounceTestInvocation(new object[] { x });
        }

        [Property]
        public async Task FallibleBooleanPropertyAsync([Between(0, 100)] int x)
        {
            await Task.CompletedTask;
            AnnounceTestInvocation(new object[] { x });
            throw new Exception("Failed!");
        }

        [Property]
        public AsyncProperty InfallibleReturnedPropertyAsync()
        {
            return Gen.Int32().Between(0, 100).ForAllAsync(async x =>
            {
                AnnounceTestInvocation(new object[] { x });
                await Task.CompletedTask;
            });
        }

        [Property]
        public AsyncProperty FallibleReturnedPropertyAsync()
        {
            return Gen.Int32().Between(0, 100).ForAllAsync(async x =>
            {
                await Task.CompletedTask;
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
        public async Task PropertyWithGenFromGenFactoryAsync(int x)
        {
            await Task.CompletedTask;
            AnnounceTestInvocation(new object[] { x });
            Assert.True(x >= 0, "They are not negative!");
        }

        private static IGen<int> EvenInt32 => Gen.Int32().Where(x => x % 2 == 0);

        [Property]
        public async Task PropertyWithGenFromMemberGenAsync([MemberGen(nameof(EvenInt32))] int x)
        {
            await Task.CompletedTask;
            AnnounceTestInvocation(new object[] { x });
            Assert.True(x % 2 != 1, "They are not odd!");
        }

        [Property]
        [InlineData(0)]
        [InlineData(1)]
        public AsyncProperty InfalliblePropertyWithControlDataAsync(int x)
        {
            return Gen.Int32().Between(0, 100).ForAllAsync(async y =>
            {
                await Task.CompletedTask;
                AnnounceTestInvocation(new object[] { x, y }, new object[] { x });
            });
        }

        [Property]
        [InlineData(0)]
        [InlineData(1)]
        public AsyncProperty FalliblePropertyWithControlDataAsync(int x)
        {
            return Gen.Int32().Between(0, 100).ForAllAsync(async y =>
            {
                await Task.CompletedTask;
                AnnounceTestInvocation(new object[] { x, y }, new object[] { x });
                return x != 1;
            });
        }

        [Sample]
        public async Task SampleVoidPropertyAsync([Between(0, 100)] int x)
        {
            await Task.CompletedTask;
            AnnounceTestInvocation(new object[] { x });
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
