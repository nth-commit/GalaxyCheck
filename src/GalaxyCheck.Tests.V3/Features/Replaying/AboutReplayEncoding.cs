using System.Runtime.InteropServices;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Gens.Parameters.Internal;
using GalaxyCheck.Runners.Replaying;

namespace GalaxyCheck_Tests_V3.Features.Replaying;

public class AboutReplayEncoding
{
    public static TheoryData<int, int, int?, IEnumerable<int>, string> Data =>
        new()
        {
            {
                0, 0, null, new[] { 0 },
                "H4sIAAAAAAAACjPQM9AzAACGS1suBQAAAA=="
            },
            {
                0, 0, null, new[] { 0, 0, 0, 0, 0, 0 },
                "H4sIAAAAAAAACjPQM9AzsIJDALKWW5IPAAAA"
            },
            {
                0, 0, null, Enumerable.Range(0, 10).Select(_ => 0),
                "H4sIAAAAAAAACjPQM9AzsMKAANukRbAXAAAA"
            },
            {
                0, 0, null, Enumerable.Range(0, 100).Select(_ => 0),
                "H4sIAAAAAAAACjPQM9AzsBoWEAC35Wk5ywAAAA=="
            },
            {
                int.MaxValue, 100, int.MaxValue, Enumerable.Range(0, 100),
                "H4sIAAAAAAAACj2Qxw0EQQzDOlo4yIn9F3bzuq8ABSpco83WfG72GU6QiKIZlsOf6HjgiQsvvPHBFz/CiOcJIgkRRTQxxBJHGunki0xSZJFNDrnkIUOOAr1GoUKNBi06yiingkrqDSqqqaGWOtpop4NOWvTb2/TQSx9jjDPBJCOmmIczzDLHGutssMmKLbbZR7vsccY5F1xy4oprbrh3xn3xP+wHz39ZWzsBAAA="
            },
        };

    [Theory]
    [MemberData(nameof(Data))]
    public void Examples(int seed, int size, int? seedWaypoint, IEnumerable<int> exampleSpacePath, string expectedEncoding)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) is false)
        {
            // TODO: Cross-platform support for this behaviour - I think this might be fixed in .NET 7
            return;
        }

        var replay = CreateReplay(seed, size, seedWaypoint, exampleSpacePath);

        var encoded = ReplayEncoding.Encode(replay);

        encoded.Should().Be(expectedEncoding);
    }

    [NebulaCheck.Property]
    public NebulaCheck.Property EncodeDecodeRoundtrip()
    {
        return NebulaCheck.Property.ForAll(DomainGen.Seed(), DomainGen.Size(), DomainGen.SeedWaypoint(), FeatureGen.ShrinkPath(), Test);

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
