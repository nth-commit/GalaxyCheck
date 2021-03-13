﻿using FluentAssertions;
using GalaxyCheck.Internal.Random;
using GalaxyCheck.Internal.Replaying;
using GalaxyCheck.Internal.Sizing;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Collections.Generic;
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
                    CreateReplay(0, 0, new [] { 0 }),
                    "H4sIAAAAAAAACjPQM9AzAACGS1suBQAAAA=="
                },
                {
                    CreateReplay(0, 0, new [] { 0, 0, 0, 0, 0, 0 }),
                    "H4sIAAAAAAAACjPQM9AzsIJDALKWW5IPAAAA"
                },
                {
                    CreateReplay(0, 0, Enumerable.Range(0, 10).Select(_ => 0)),
                    "H4sIAAAAAAAACjPQM9AzsMKAANukRbAXAAAA"
                },
                {
                    CreateReplay(0, 0, Enumerable.Range(0, 100).Select(_ => 0)),
                    "H4sIAAAAAAAACjPQM9AzsBoWEAC35Wk5ywAAAA=="
                },
                {
                    CreateReplay(int.MaxValue, 100, Enumerable.Range(0, 100)),
                    "H4sIAAAAAAAACg2Qxw3AQBACO7I2sGn6L8z3RQIGwjXabM3nZp/hBIkommE5/ImOB5648MIbH3zxI4x4niCSEFFEE0MscaSRTr7IJEUW2eSQSx4y5CjQaxQq1GjQoqOMciqopB5QUU0NtdTRRjsddNKiH2/TQy99jDHOBJOMmGLenGGWOdZYZ4NNVmyxzb61yx5nnHPBJSeuuOaGe2fcD7ogWEAwAQAA"
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
                var replay0 = CreateReplay(seed, size, exampleSpacePath);
                var replay1 = ReplayEncoding.Decode(ReplayEncoding.Encode(replay0));

                replay1.GenParameters.Should().Be(replay0.GenParameters);
                replay1.ExampleSpacePath.Should().BeEquivalentTo(replay0.ExampleSpacePath);
            });

        private static Replay CreateReplay(int seed, int size, IEnumerable<int> exampleSpacePath) =>
            new Replay(new GalaxyCheck.GenParameters(Rng.Create(seed), new Size(size)), exampleSpacePath);
    }
}