namespace GalaxyCheck
{
    using GalaxyCheck.Gens;
    using System;

    public static partial class Gen
    {
        /// <summary>
        /// Generates bytes. By default, the generator produces bytes from 0x00 that will grow towards 0xFF, as the
        /// generator size increases. However, byte generation can be customised using the builder methods on
        /// <see cref="IIntGen{byte}"/>.
        /// <returns>The new generator.</returns>
        public static IIntGen<byte> Byte() => new ByteGen();
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Constrains the generator so that it only produces values between the supplied range (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<byte> Between(this IIntGen<byte> gen, byte x, byte y)
        {
            var min = x > y ? y : x;
            var max = x > y ? x : y;
            return gen.GreaterThanEqual(min).LessThanEqual(max);
        }

        /// <summary>
        /// Constrains the generator so that it only produces values less than the supplied maximum. 
        /// </summary>
        /// <param name="maxExclusive">The maximum byte to generate (exclusive).</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<byte> LessThan(this IIntGen<byte> gen, byte maxExclusive)
        {
            try
            {
                checked
                {
                    return gen.LessThanEqual((byte)(maxExclusive - 1));
                }
            }
            catch (OverflowException ex)
            {
                return new FatalIntGen<byte>(ex);
            }
        }

        /// <summary>
        /// Constrains the generator so that it only produces values greater than the supplied minimum.
        /// </summary>
        /// <param name="minExclusive">The minimum byte to generate (exclusive).</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<byte> GreaterThan(this IIntGen<byte> gen, byte minExclusive)
        {
            try
            {
                checked
                {
                    return gen.GreaterThanEqual((byte)(minExclusive + 1));
                }
            }
            catch (OverflowException ex)
            {
                return new FatalIntGen<byte>(ex);
            }
        }
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using System.Collections.Generic;

    internal record ByteGen(
        byte? Min = null,
        byte? Max = null,
        byte? Origin = null,
        Gen.Bias? Bias = null) : GenProvider<byte>, IIntGen<byte>
    {
        public IIntGen<byte> GreaterThanEqual(byte min) => this with { Min = min };

        public IIntGen<byte> LessThanEqual(byte max) => this with { Max = max };

        public IIntGen<byte> ShrinkTowards(byte origin) => this with { Origin = origin };

        public IIntGen<byte> WithBias(Gen.Bias bias) => this with { Bias = bias };

        protected override IGen<byte> Get
        {
            get
            {
                var gen = Gen
                    .Int16()
                    .GreaterThanEqual(Min ?? byte.MinValue)
                    .LessThanEqual(Max ?? byte.MaxValue);

                if (Origin != null)
                {
                    gen = gen.ShrinkTowards(Origin.Value);
                }

                if (Bias != null)
                {
                    gen = gen.WithBias(Bias.Value);
                }

                return gen
                    .Select(x => (byte)x)
                    .SelectError(error => new GenErrorData(error.Message));
            }
        }
    }
}
