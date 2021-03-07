using GalaxyCheck.Internal.ExampleSpaces;
using Snapshooter.Xunit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Xunit;

namespace Tests.V2.ShrinkTests
{
    public class AboutTowardsCount : IClassFixture<AboutTowardsCount.Fixture>
    {
        private readonly Fixture _fixture;

        public AboutTowardsCount(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Snapshots_TowardsCount()
        {
            var result = _fixture
                .Scenarios
                .Select(input =>
                {
                    var func = ShrinkFunc.TowardsCount<int, int>(input.MinLength, x => x);

                    var shrinks = func(input.List);

                    return new
                    {
                        Input = input,
                        Output = shrinks
                    };
                })
                .ToList();

            Snapshot.Match(result);
        }

        [Fact]
        public void Snapshots_TowardsCountOptimized()
        {
            var result = _fixture
                .Scenarios
                .Select(scenario =>
                {
                    var func = ShrinkFunc.TowardsCountOptimized<int, int>(scenario.MinLength, x => x);

                    var shrinks = func(scenario.List);

                    return new
                    {
                        Input = scenario,
                        Output = shrinks
                    };
                })
                .ToList();

            Snapshot.Match(result);
        }

        public record ListShrinkingScenario(List<int> List, int MinLength);

        public class Fixture
        {
            public List<ListShrinkingScenario> Scenarios { get; init; }

            public Fixture()
            {
                var path = Path.Combine(GetTestDirectory(), "./Fixtures/list-shrinking-scenarios.json");
                var json = File.ReadAllText(path);
                Scenarios = JsonSerializer.Deserialize<List<ListShrinkingScenario>>(json)!;
            }

            private static string GetTestDirectory([CallerFilePath] string path = null) => Path.GetDirectoryName(path)!;
        }
    }
}
