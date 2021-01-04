using GalaxyCheck.Abstractions;
using System;

namespace GalaxyCheck.Random
{
    public record Rng : IRng
    {
        /// <summary>
        /// Creates a new RNG with a random seed.
        /// </summary>
        /// <returns>The new RNG.</returns>
        public static IRng Spawn() => new Rng(new System.Random().Next(), 0);

        /// <summary>
        /// Create a new RNG with the given seed.
        /// </summary>
        /// <param name="seed">The seed for the RNG.</param>
        /// <returns>The new RNG.</returns>
        public static IRng Create(int seed) => new Rng(seed, 0);

        private System.Random Random => new System.Random(Seed);

        public int Seed { get; init; }

        public int Order { get; init; }

        private Rng(int seed, int order)
        {
            Seed = seed;
            Order = order;
        }

        public IRng Next() => new Rng(Random.Next(), Order + 1);

        public int Value(int min, int max)
        {
            if (min > max) throw new ArgumentOutOfRangeException(nameof(min), "'min' cannot be greater than 'max'");

            var maxOffset = max == int.MaxValue ? max : max + 1;

            return Random.Next(min, maxOffset);
        }
    }
}
