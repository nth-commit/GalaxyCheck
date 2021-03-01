using GalaxyCheck;
using GalaxyCheck.Xunit;
using Newtonsoft.Json;
using System;
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
        public void InfallibleVoidProperty(int x)
        {
            AnnounceTestInvocation(nameof(InfallibleVoidProperty));
        }

        [Property]
        public void FallibleVoidProperty(int x)
        {
            AnnounceTestInvocation(nameof(FallibleVoidProperty));
            throw new Exception("Failed!");
        }

        [Property]
        public void InfallibleBooleanProperty(int x)
        {
            AnnounceTestInvocation(nameof(InfallibleBooleanProperty));
        }

        [Property]
        public void FallibleBooleanProperty(int x)
        {
            AnnounceTestInvocation(nameof(FallibleBooleanProperty));
            throw new Exception("Failed!");
        }

        [Property]
        public Property InfallibleNestedProperty(int x)
        {
            AnnounceTestInvocation(nameof(InfallibleNestedProperty));
            return Gen.Int32().ForAll(y => { });
        }

        [Property]
        public Property FallibleNestedProperty(int x)
        {
            AnnounceTestInvocation(nameof(FallibleNestedProperty));
            return Gen.Int32().ForAll(y => { throw new Exception("Failed!"); });
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
