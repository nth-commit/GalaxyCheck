namespace GalaxyCheck
{
    using GalaxyCheck.Gens;
    using System;

    public static partial class Gen
    {
        /// <summary>
        /// Generates 32-bit integers (ints). By default, the generator produces ints from 0, that grow towards
        /// <see cref="int.MinValue"/> and <see cref="int.MaxValue"/>, as the generator size increases. However,
        /// int generation can be customised using the builder methods on <see cref="IIntGen{int}"/>.
        /// <returns>The new generator.</returns>
        public static IIntGen<int> Int32() => new Int32Gen(nameof(Int32));
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Constrains the generator so that it only produces values between the supplied range (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<int> Between(this IIntGen<int> gen, int x, int y)
        {
            var min = x > y ? y : x;
            var max = x > y ? x : y;
            return gen.GreaterThanEqual(min).LessThanEqual(max);
        }

        /// <summary>
        /// Constrains the generator so that it only produces values less than the supplied maximum. 
        /// </summary>
        /// <param name="maxExclusive">The maximum int to generate (exclusive).</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<int> LessThan(this IIntGen<int> gen, int maxExclusive)
        {
            try
            {
                checked
                {
                    return gen.LessThanEqual(maxExclusive - 1);
                }
            }
            catch (OverflowException ex)
            {
                return new FatalIntGen<int>($"{nameof(Gen.Int32)}().{nameof(LessThan)}({maxExclusive})", ex);
            }
        }

        /// <summary>
        /// Constrains the generator so that it only produces values greater than the supplied minimum.
        /// </summary>
        /// <param name="minExclusive">The minimum integer to generate (exclusive).</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<int> GreaterThan(this IIntGen<int> gen, int minExclusive)
        {
            try
            {
                checked
                {
                    return gen.GreaterThanEqual(minExclusive + 1);
                }
            }
            catch (OverflowException ex)
            {
                return new FatalIntGen<int>($"{nameof(Gen.Int32)}().{nameof(GreaterThan)}({minExclusive})", ex);
            }
        }
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;

    internal record Int32Gen(
        string PublicGenName,
        int? Min = null,
        int? Max = null,
        int? Origin = null,
        Gen.Bias? Bias = null) : GenProvider<int>, IIntGen<int>
    {
        public IIntGen<int> GreaterThanEqual(int min) => this with { Min = min };

        public IIntGen<int> LessThanEqual(int max) => this with { Max = max };

        public IIntGen<int> ShrinkTowards(int origin) => this with { Origin = origin };

        public IIntGen<int> WithBias(Gen.Bias bias) => this with { Bias = bias };

        protected override IGen<int> Get
        {
            get
            {
                var gen = Gen
                    .Int64()
                    .GreaterThanEqual(Min ?? int.MinValue)
                    .LessThanEqual(Max ?? int.MaxValue);

                if (Origin != null)
                {
                    gen = gen.ShrinkTowards(Origin.Value);
                }

                if (Bias != null)
                {
                    gen = gen.WithBias(Bias.Value);
                }

                return gen
                    .Select(x => (int)x)
                    .SelectError(error => new GenErrorData(PublicGenName, error.Message));
            }
        }
    }
}
