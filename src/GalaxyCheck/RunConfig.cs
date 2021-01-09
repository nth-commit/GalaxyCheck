using GalaxyCheck.Sizing;

namespace GalaxyCheck
{
    public record RunConfig
    {
        public int? Iterations { get; init; }

        public int? Seed { get; init; }

        public Size? Size { get; init; }

        public IRng Rng => Seed == null ? Random.Rng.Spawn() : Random.Rng.Create(Seed.Value);

        public RunConfig(int? iterations = null, int? seed = null, Size? size = null)
        {
            Iterations = iterations;
            Seed = seed;
            Size = size;
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
