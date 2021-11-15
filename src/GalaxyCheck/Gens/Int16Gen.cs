namespace GalaxyCheck
{
    using GalaxyCheck.Gens;
    using System;

    public static partial class Gen
    {
        /// <summary>
        /// Generates 16-bit integers (shorts). By default, the generator produces shorts from 0, that grow towards
        /// <see cref="short.MinValue"/> and <see cref="short.MaxValue"/>, as the generator size increases. However,
        /// short generation can be customised using the builder methods on <see cref="IIntGen{short}"/>.
        /// <returns>The new generator.</returns>
        public static IIntGen<short> Int16() => new Int16Gen(nameof(Int16));
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Constrains the generator so that it only produces values between the supplied range (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<short> Between(this IIntGen<short> gen, short x, short y)
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
        public static IIntGen<short> LessThan(this IIntGen<short> gen, short maxExclusive)
        {
            try
            {
                checked
                {
                    return gen.LessThanEqual((short)(maxExclusive - 1));
                }
            }
            catch (OverflowException ex)
            {
                return new FatalIntGen<short>($"{nameof(Gen.Int16)}().{nameof(LessThan)}({maxExclusive})", ex);
            }
        }

        /// <summary>
        /// Constrains the generator so that it only produces values greater than the supplied minimum.
        /// </summary>
        /// <param name="minExclusive">The minimum integer to generate (exclusive).</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<short> GreaterThan(this IIntGen<short> gen, short minExclusive)
        {
            try
            {
                checked
                {
                    return gen.GreaterThanEqual((short)(minExclusive + 1)); ;
                }
            }
            catch (OverflowException ex)
            {
                return new FatalIntGen<short>($"{nameof(Gen.Int16)}().{nameof(GreaterThan)}({minExclusive})", ex);
            }
        }
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;

    internal record Int16Gen(
        string PublicGenName,
        short? Min = null,
        short? Max = null,
        short? Origin = null,
        Gen.Bias? Bias = null) : GenProvider<short>, IIntGen<short>
    {
        public IIntGen<short> GreaterThanEqual(short min) => this with { Min = min };

        public IIntGen<short> LessThanEqual(short max) => this with { Max = max };

        public IIntGen<short> ShrinkTowards(short origin) => this with { Origin = origin };

        public IIntGen<short> WithBias(Gen.Bias bias) => this with { Bias = bias };

        protected override IGen<short> Get
        {
            get
            {
                var gen = Gen
                    .Int32()
                    .GreaterThanEqual(Min ?? short.MinValue)
                    .LessThanEqual(Max ?? short.MaxValue);

                if (Origin != null)
                {
                    gen = gen.ShrinkTowards(Origin.Value);
                }

                if (Bias != null)
                {
                    gen = gen.WithBias(Bias.Value);
                }

                return gen
                    .Select(x => (short)x)
                    .SelectError(error => new GenErrorData(PublicGenName, error.Message));
            }
        }
    }
}
