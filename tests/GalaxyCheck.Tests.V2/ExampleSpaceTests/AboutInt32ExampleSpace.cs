using GalaxyCheck.ExampleSpaces;
using Snapshooter;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Xunit;

namespace Tests.V2.ExampleSpaceTests
{
    public class AboutInt32ExampleSpace : IClassFixture<AboutInt32ExampleSpace.Fixture>
    {
        private readonly Fixture _fixture;

        public AboutInt32ExampleSpace(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Snapshots(bool isOptimized)
        {
            var tests = _fixture
                .Scenarios
                .Select(scenario =>
                {
                    Func<int, int, int, int, IExampleSpace<int>> exampleSpaceFactory = isOptimized
                        ? ExampleSpaceFactory.Int32Optimized
                        : ExampleSpaceFactory.Int32;

                    var exampleSpace = exampleSpaceFactory(
                        scenario.Value,
                        scenario.Origin,
                        scenario.Min,
                        scenario.Max);

                    var rendering = exampleSpace.Render(x => x.ToString());

                    return new
                    {
                        Input = scenario,
                        Output = rendering
                    };
                })
                .ToList();

            foreach (var test in tests)
            {
                var nameExtensionBase = $"Value={test.Input.Value};Origin={test.Input.Origin}Min={test.Input.Min};Max={test.Input.Max}";
                var nameExtension = isOptimized ? nameExtensionBase : nameExtensionBase + $";Unoptimized";
                var input = test.Input;
                Snapshot.Match(test.Output, SnapshotNameExtension.Create(nameExtension));
            }
        }

        public record Int32ExampleSpaceScenario(int Min, int Max, int Origin, int Value);

        public class Fixture
        {
            public List<Int32ExampleSpaceScenario> Scenarios { get; init; }

            public Fixture()
            {
                var path = Path.Combine(GetTestDirectory(), "./Fixtures/int32-example-space-scenarios.json");
                var json = File.ReadAllText(path);
                Scenarios = JsonSerializer.Deserialize<List<Int32ExampleSpaceScenario>>(json)!;
            }

            private static string GetTestDirectory([CallerFilePath] string path = null!) => Path.GetDirectoryName(path)!;
        }
    }
}
