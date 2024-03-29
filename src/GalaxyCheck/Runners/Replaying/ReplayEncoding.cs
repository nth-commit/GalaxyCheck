﻿using GalaxyCheck.Gens.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GalaxyCheck.Runners.Replaying
{
    internal static class ReplayEncoding
    {
        private static string EncodeInt(int seed) => seed.ToString();
        private static int DecodeInt(string str) => int.Parse(str);

        private static string EncodeExampleSpacePath(IEnumerable<int> exampleSpacePath) =>
            string.Join(":", exampleSpacePath.Select(EncodeInt));
        private static IEnumerable<int> DecodeExampleSpacePath(string str) =>
            str.Split(':').Where(x => x != "").Select(DecodeInt);

        private static string CompressString(string str)
        {
            using var msIn = new MemoryStream(Encoding.ASCII.GetBytes(str));
            using var msOut = new MemoryStream();
            using var gzs = new GZipStream(msOut, CompressionMode.Compress);

            msIn.CopyTo(gzs);
            gzs.Close();

            return Convert.ToBase64String(msOut.ToArray());
        }

        private static string DecompressString(string str)
        {
            using var msIn = new MemoryStream(Convert.FromBase64String(str));
            using var gzs = new GZipStream(msIn, CompressionMode.Decompress);
            using var msOut = new MemoryStream();

            gzs.CopyTo(msOut);

            return Encoding.ASCII.GetString(msOut.ToArray());
        }

        public static string Encode(Replay replay)
        {
            var encodingComponents = new List<string>()
            {
                EncodeInt(replay.GenParameters.Rng.Seed),
                EncodeInt(replay.GenParameters.Size.Value),
                EncodeExampleSpacePath(replay.ExampleSpacePath)
            };

            if (replay.GenParameters.RngWaypoint != null)
            {
                encodingComponents.Add(EncodeInt(replay.GenParameters.RngWaypoint.Seed));
            }

            return CompressString(string.Join(".", encodingComponents));
        }

        public static Replay Decode(string str)
        {
            var decompressed = DecompressString(str);

            var components = decompressed.Split('.');

            var seed = DecodeInt(components[0]);
            var size = DecodeInt(components[1]);
            var exampleSpacePath = DecodeExampleSpacePath(components[2]);

            int? seedWapoint = components.Count() > 3 ? DecodeInt(components[3]) : null;

            return new Replay(
                GenParameters.Parse(seed, size, seedWapoint),
                exampleSpacePath);
        }
    }
}
