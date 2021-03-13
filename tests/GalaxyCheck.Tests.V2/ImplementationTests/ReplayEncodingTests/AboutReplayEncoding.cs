using FluentAssertions;
using GalaxyCheck.Internal.Replaying;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace Tests.V2.ImplementationTests.ReplayEncodingTests
{
    public class AboutReplayEncoding
    {
        public static TheoryData<Replay, string> Data =>
            new TheoryData<Replay, string>
            {
                {
                    new Replay(0, 0, new [] { 0 }),
                    "H4sIAAAAAAAACjPQM9AzAACGS1suBQAAAA=="
                },
                {
                    new Replay(0, 0, new [] { 0, 0, 0, 0, 0, 0 }),
                    "H4sIAAAAAAAACjPQM9AzsIJDALKWW5IPAAAA"
                },
                {
                    new Replay(0, 0, Enumerable.Range(0, 10).Select(_ => 0)),
                    "H4sIAAAAAAAACjPQM9AzsMKAANukRbAXAAAA"
                },
                {
                    new Replay(0, 0, Enumerable.Range(0, 100).Select(_ => 0)),
                    "H4sIAAAAAAAACjPQM9AzsBoWEAC35Wk5ywAAAA=="
                },
                {
                    new Replay(int.MaxValue, int.MaxValue, Enumerable.Range(0, 100)),
                    "H4sIAAAAAAAACkWQtw0EQQwDO3rIUG76L+w2+4wgQBuu0WZrfvGHhhMkomiG5fBHOh544sILb3zwxY8w4mmCSEJEEU0MscSRRjr5LJMUWWSTQy55yJCjQC9RqFCjQYuOMsqpoJJ6hYpqaqiljjba6aCTFv36Nj300scY40wwyYgp5s0ZZpljjXU22GTFFtvsW7vsccY5F1xy4oprbrh3xn1aP3t8NwEAAA=="
                },
            };

        [Theory]
        [MemberData(nameof(Data))]
        public void Examples(Replay replay, string expectedEncoding)
        {
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
            from exampleSpacePath in Gen.Int32().ListOf()
            select Property.ForThese(() =>
            {
                var replay0 = new Replay(seed, size, exampleSpacePath);
                var replay1 = ReplayEncoding.Decode(ReplayEncoding.Encode(replay0));

                replay1.Seed.Should().Be(replay0.Seed);
                replay1.Size.Should().Be(replay0.Size);
                replay1.ExampleSpacePath.Should().BeEquivalentTo(replay0.ExampleSpacePath);
            });
    }
}
