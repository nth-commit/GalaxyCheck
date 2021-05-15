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

        [Fact]
        public void Snapshots()
        {
            var tests = _fixture
                .Scenarios
                .Select(scenario =>
                {
                    var exampleSpace = ExampleSpaceFactory.Int32(
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
                var nameExtension = $"Value={test.Input.Value};Origin={test.Input.Origin}Min={test.Input.Min};Max={test.Input.Max}";
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
