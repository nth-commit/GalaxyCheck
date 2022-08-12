using FluentAssertions;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Gens.Parameters.Internal;
using GalaxyCheck.Runners.Replaying;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace Tests.V2.ImplementationTests.ReplayEncodingTests
{
    public class AboutReplayEncoding
    {
        public static TheoryData<int, int, int?, IEnumerable<int>, string> Data =>
            new TheoryData<int, int, int?, IEnumerable<int>, string>
            {
                {
                    0, 0, null, new [] { 0 },
                    "H4sIAAAAAAAACjPQM9AzAACGS1suBQAAAA=="
                },
                {
                    0, 0, null, new [] { 0, 0, 0, 0, 0, 0 },
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
            var replay = CreateReplay(seed, size, seedWaypoint, exampleSpacePath);

            // TODO: Cross-platform consistent replay encoding
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var encoded = ReplayEncoding.Encode(replay);

                encoded.Should().Be(expectedEncoding);
            }
        }

        [Property]
        public IGen<Test> EncodeDecodeIsAnIsomorphism() =>
            from seed in Gen.Int32()
            from size in Gen.Int32().Between(0, 100)
            from seedWaypoint in Gen.Choose(Gen.Int32().Cast<int?>(), Gen.Constant<int?>(null))
            from exampleSpacePath in Gen.Int32().ListOf()
            select Property.ForThese(() =>
            {
                var replay0 = CreateReplay(seed, size, seedWaypoint, exampleSpacePath);
                var replay1 = ReplayEncoding.Decode(ReplayEncoding.Encode(replay0));

                replay1.GenParameters.Should().Be(replay0.GenParameters);
                replay1.ExampleSpacePath.Should().BeEquivalentTo(replay0.ExampleSpacePath);
            });

        private static Replay CreateReplay(int seed, int size, int? seedWaypoint, IEnumerable<int> exampleSpacePath) =>
            new Replay(new GenParameters(Rng.Create(seed), new Size(size), seedWaypoint == null ? null : Rng.Create(seedWaypoint.Value)), exampleSpacePath);
    }
}
