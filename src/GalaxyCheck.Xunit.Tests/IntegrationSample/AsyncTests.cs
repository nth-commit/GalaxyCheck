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
        public IGen<TestAsync> InfalliblePureProperty() =>
            from a in Gen.Int32().Between(0, 100)
            select Property.ForTheseAsync(async () =>
            {
                await Task.Delay(1);
                AnnounceTestInvocation(nameof(InfalliblePureProperty));
            });

        [Property]
        public IGen<TestAsync> FalliblePureProperty() =>
            from a in Gen.Int32().Between(0, 100)
            select Property.ForTheseAsync(async () =>
            {
                await Task.Delay(1);
                AnnounceTestInvocation(nameof(FalliblePureProperty));
                throw new Exception("Failed!");
            });

        [Property]
        public async Task InfallibleVoidProperty([Between(0, 100)] int x)
        {
            await Task.Delay(1);
            AnnounceTestInvocation(nameof(InfallibleVoidProperty));
        }

        [Property]
        public async Task FallibleVoidProperty([Between(0, 100)] int x)
        {
            await Task.Delay(1);
            AnnounceTestInvocation(nameof(FallibleVoidProperty));
            throw new Exception("Failed!");
        }

        [Property]
        public async Task InfallibleBooleanProperty([Between(0, 100)] int x)
        {
            await Task.Delay(1);
            AnnounceTestInvocation(nameof(InfallibleBooleanProperty));
        }

        [Property]
        public async Task FallibleBooleanProperty([Between(0, 100)] int x)
        {
            await Task.Delay(1);
            AnnounceTestInvocation(nameof(FallibleBooleanProperty));
            throw new Exception("Failed!");
        }

        //[Property]
        //public async Property InfallibleNestedProperty([Between(0, 100)] int x)
        //{
        //    await Task.Delay(1);
        //    AnnounceTestInvocation(nameof(InfallibleNestedProperty));
        //    return Gen.Int32().Between(0, 100).ForAll(y => { });
        //}

        //[Property]
        //public async Property FallibleNestedProperty([Between(0, 100)] int x)
        //{
        //    await Task.Delay(1);
        //    AnnounceTestInvocation(nameof(FallibleNestedProperty));
        //    return Gen.Int32().Between(0, 100).ForAll(y => { throw new Exception("Failed!"); });
        //}

        public class GenFactoryWhereIntsAreNonNegativeAttribute : GenFactoryAttribute
        {
            public override IGenFactory Value => Gen.Factory().RegisterType(Gen.Int32().GreaterThanEqual(0));
        }

        [Property]
        [GenFactoryWhereIntsAreNonNegative]
        public async Task PropertyWithGenFromGenFactory(int x)
        {
            await Task.Delay(1);
            AnnounceTestInvocation(nameof(PropertyWithGenFromGenFactory), new [] { x });
            Assert.True(x >= 0, "They are not negative!");
        }

        private static IGen<int> EvenInt32 => Gen.Int32().Where(x => x % 2 == 0);

        [Property]
        public async Task PropertyWithGenFromMemberGen([MemberGen(nameof(EvenInt32))] int x)
        {
            await Task.Delay(1);
            AnnounceTestInvocation(nameof(PropertyWithGenFromMemberGen), new[] { x });
            Assert.True(x % 2 != 1, "They are not odd!");
        }

        [Sample]
        public IGen<TestAsync> Sample() =>
            from a in Gen.Int32().Between(0, 100)
            select Property.ForTheseAsync(async () =>
            {
                await Task.Delay(1);
            });

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
