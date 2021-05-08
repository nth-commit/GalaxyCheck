namespace GalaxyCheck
{
    using GalaxyCheck.Gens;

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
        public static IIntGen<byte> LessThan(this IIntGen<byte> gen, byte maxExclusive) => gen.LessThanEqual((byte)(maxExclusive - 1));

        /// <summary>
        /// Constrains the generator so that it only produces values greater than the supplied minimum.
        /// </summary>
        /// <param name="minExclusive">The minimum integer to generate (exclusive).</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<byte> GreaterThan(this IIntGen<byte> gen, int minExclusive) => gen.GreaterThanEqual((byte)(minExclusive + 1));
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using System.Collections.Generic;

    public interface IIntGen<T> : IGen<T>
    {
        /// <summary>
        /// Constrains the generator so that it only produces values greater than or equal to the supplied minimum.
        /// </summary>
        /// <param name="min">The minimum integer to generate.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IIntGen<T> GreaterThanEqual(T min);

        /// <summary>
        /// Constrains the generator so that it only produces values less than or equal to the supplied maximum.
        /// </summary>
        /// <param name="max">The maximum integer to generate.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IIntGen<T> LessThanEqual(T max);

        /// <summary>
        /// Modifies the generator so that values will ultimately shrink to the supplied origin. The origin is the
        /// "smallest" value that all values should shrink towards. The origin must be within the the bounds of the
        /// generator. If not otherwise specified, the generator will try and infer the most sensible origin, based
        /// upon other constraints.
        /// <param name="origin">The "smallest" value that generated values should shrink towards.</param>
        /// <returns>A new generator with the origin applied.</returns>
        IIntGen<T> ShrinkTowards(T origin);

        /// <summary>
        /// Modifies how the generator biases values with respect to the size parameter.
        /// </summary>
        /// <returns>A new generator with the biasing effect applied.</returns>
        IIntGen<T> WithBias(Gen.Bias bias);
    }

    internal class ByteGen : BaseGen<byte>, IIntGen<byte>
    {
        private readonly ByteGenConfig _config;

        private record ByteGenConfig(
            byte? Min,
            byte? Max,
            byte? Origin,
            Gen.Bias? Bias);

        private ByteGen(ByteGenConfig config)
        {
            _config = config;
        }

        public ByteGen() : this(new ByteGenConfig(Min: null, Max: null, Origin: null, Bias: null))
        {
        }

        public IIntGen<byte> GreaterThanEqual(byte min) => new ByteGen(_config with { Min = min });

        public IIntGen<byte> LessThanEqual(byte max) => new ByteGen(_config with { Max = max });

        public IIntGen<byte> ShrinkTowards(byte origin) => new ByteGen(_config with { Origin = origin });

        public IIntGen<byte> WithBias(Gen.Bias bias) => new ByteGen(_config with { Bias = bias });

        protected override IEnumerable<IGenIteration<byte>> Run(GenParameters parameters) =>
            CreateGen(_config).Advanced.Run(parameters);

        private static IGen<byte> CreateGen(ByteGenConfig config)
        {
            var gen = Gen
                .Int32()
                .GreaterThanEqual(config.Min ?? byte.MinValue)
                .LessThanEqual(config.Max ?? byte.MaxValue);

            if (config.Origin != null)
            {
                gen = gen.ShrinkTowards(config.Origin.Value);
            }

            if (config.Bias != null)
            {
                gen = gen.WithBias(config.Bias.Value);
            }

            return gen
                .Select(x => (byte)x)
                .SelectError(error => new GenErrorData(nameof(ByteGen), error.Message));
        }
    }
}
