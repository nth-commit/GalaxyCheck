using System;

namespace GalaxyCheck.Gens.Parameters.Internal
{
    internal record Rng : IRng
    {
        /// <summary>
        /// Creates a new RNG with a random seed.
        /// </summary>
        /// <returns>The new RNG.</returns>
        public static IRng Spawn() => Create(new System.Random().Next());

        /// <summary>
        /// Create a new RNG with the given seed.
        /// </summary>
        /// <param name="seed">The seed for the RNG.</param>
        /// <returns>The new RNG.</returns>
        public static IRng Create(int seed) => new Rng(seed, seed, 0);

        private System.Random Random => new System.Random(Seed);

        public int Family { get; init; }

        public int Seed { get; init; }

        public int Order { get; init; }

        private Rng(int family, int seed, int order)
        {
            Family = family;
            Seed = seed;
            Order = order;
        }

        public IRng Next() => new Rng(Family, Random.Next(), Order + 1);

        public IRng Fork() => Create((Family + 1) * -1521134295 + Order);

        public IRng Influence(int seed)
        {
            int nextSeed;
            unchecked
            {
                nextSeed = Seed + seed;
            }
            return new Rng(Family, nextSeed, Order);
        }

        public long Value(long min, long max)
        {
            if (min > max) throw new ArgumentOutOfRangeException(nameof(min), "'min' cannot be greater than 'max'");

            var maxOffset = max == long.MaxValue ? max : max + 1;

            return Random.NextLong(min, maxOffset);
        }

        public override string ToString()
        {
            return "[" + string.Join(",", new [] { Family, Seed, Order }) + "]";
        }
    }
}
