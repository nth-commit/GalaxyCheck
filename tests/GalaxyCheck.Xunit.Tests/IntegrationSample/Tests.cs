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
        public void InfallibleVoidProperty()
        {
            AnnounceTestInvocation(nameof(InfallibleVoidProperty));
        }

        [Property]
        public void FallibleVoidProperty()
        {
            AnnounceTestInvocation(nameof(FallibleVoidProperty));
            throw new Exception("Failed!");
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
