﻿namespace GalaxyCheck.Gens.Parameters
{
    /// <summary>
    /// A side-effectless random number generator for 32-bit integers.
    /// </summary>
    public interface IRng
    {
        /// <summary>
        /// Identifies the lineage of this RNG. The family will always be the seed of the original RNG.
        /// </summary>
        int Family { get; }

        /// <summary>
        /// The constant seed used to generate integers from this RNG.
        /// </summary>
        int Seed { get; }

        /// <summary>
        /// The order of this RNG. After creating an initial RNG, the order tracks how many generations of randomness
        /// have passed. That is, how many times Next() has been called. This is a useful metric to approximate
        /// performance at a high-level. Knowing this also allows us to permutate randomness cleverly whilst shrinking.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Creates a new RNG based off of this one. This is a pure function and will always return an RNG with the
        /// same new seed.
        /// </summary>
        /// <returns>The new RNG.</returns>
        IRng Next();

        /// <summary>
        /// Creates a new RNG based off of, but divorced from, this one. Like <see cref="Next"/>, but the returned RNG
        /// will be of a different family, and have it's order reset.
        /// </summary>
        /// <returns></returns>
        IRng Fork();

        /// <summary>
        /// Generates a random integer, based off of the seed, and some given boundaries. This is a pure function and
        /// will always return the same integer, given the same boundaries.
        /// </summary>
        /// <param name="min">The minimum integer to generate.</param>
        /// <param name="max">The maximum integer to generate.</param>
        /// <returns>A new, random integer.</returns>
        int Value(int min, int max);
    }
}