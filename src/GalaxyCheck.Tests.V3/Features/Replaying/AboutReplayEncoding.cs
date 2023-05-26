using System.Runtime.InteropServices;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Gens.Parameters.Internal;
using GalaxyCheck.Runners.Replaying;

namespace GalaxyCheck.Tests.Features.Replaying;

public class AboutReplayEncoding
{
    public static TheoryData<int, int, int?, IEnumerable<int>, string> Data =>
        new()
        {
            {
                0, 0, null, new[] { 0 },
                "M9Az0DMAAA=="
            },
            {
                0, 0, null, new[] { 0, 0, 0, 0, 0, 0 },
                "M9Az0DOwgkMA"
            },
            {
                0, 0, null, Enumerable.Range(0, 10).Select(_ => 0),
                "M9Az0DOwwoAA"
            },
            {
                0, 0, null, Enumerable.Range(0, 100).Select(_ => 0),
                "M9Az0DOwGhYQAA=="
            },
            {
                int.MaxValue, 100, int.MaxValue, Enumerable.Range(0, 100),
                "PZDHDQRBDMM6WjjIif0XdvO6rwAFKlyjzdZ8bvYZTpCIohmWw5/oeOCJCy+88cEXP8KI5wkiCRFFNDHEEkca6eSLTFJkkU0OueQhQ44CvUahQo0GLTrKKKeCSuoNKqqpoZY62ming05a9Nvb9NBLH2OMM8EkI6aYhzPMMsca62ywyYotttlHu+xxxjkXXHLiimtuuHfGffE/7Ac="
            },
        };

    [Theory]
    [MemberData(nameof(Data))]
    public void Examples(int seed, int size, int? seedWaypoint, IEnumerable<int> exampleSpacePath, string expectedEncoding)
    {
        var replay = CreateReplay(seed, size, seedWaypoint, exampleSpacePath);

        var encoded = ReplayEncoding.Encode(replay);

        encoded.Should().Be(expectedEncoding);
    }

    [Stable.Property]
    public Stable.Property EncodeDecodeRoundtrip()
    {
        return Stable.Property.ForAll(DomainGen.Seed(), DomainGen.Size(), DomainGen.SeedWaypoint(), FeatureGen.ShrinkPath(), Test);

        static void Test(int seed, int size, int? seedWaypoint, IEnumerable<int> exampleSpacePath)
        {
            var replay0 = CreateReplay(seed, size, seedWaypoint, exampleSpacePath);
            var replay1 = ReplayEncoding.Decode(ReplayEncoding.Encode(replay0));

            replay1.Should().BeEquivalentTo(replay0);
        }
    }

    private static Replay CreateReplay(int seed, int size, int? seedWaypoint, IEnumerable<int> exampleSpacePath) =>
        new(new GenParameters(Rng.Create(seed), new Size(size), seedWaypoint == null ? null : Rng.Create(seedWaypoint.Value)), exampleSpacePath);
}
