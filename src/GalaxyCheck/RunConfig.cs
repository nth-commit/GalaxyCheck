using GalaxyCheck.Sizing;
using System;

namespace GalaxyCheck
{
    public record RunConfig
    {
        public int? Iterations { get; init; }

        public int? Seed { get; init; }

        public Size? Size { get; init; }

        public Func<int, IRng>? MakeRng { get; init; }

        public IRng Rng
        {
            get
            {
                var seed = Seed == null ? Random.Rng.Spawn().Seed : Seed.Value;
                var makeRng = MakeRng == null ? Random.Rng.Create : MakeRng;
                return makeRng(seed);
            }
        }

        public RunConfig(
            int? iterations = null,
            int? seed = null,
            Size? size = null,
            Func<int, IRng>? makeRng = null)
        {
            Iterations = iterations;
            Seed = seed;
            Size = size;
            MakeRng = makeRng;
        }

        public RunConfig WithIterations(int iterations) => new RunConfig(
            iterations: iterations,
            seed: Seed,
            size: Size);

        public RunConfig WithSeed(int seed) => new RunConfig(
            iterations: Iterations,
            seed: seed,
            size: Size);

        public RunConfig WithSize(Size size) => new RunConfig(
            iterations: Iterations,
            seed: Seed,
            size: size);
    }
}
