namespace GalaxyCheck
{
    using GalaxyCheck.Gens;

    public static partial class Gen
    {
        /// <summary>
        /// Generates 16-bit integers (shorts). By default, the generator produces shorts from 0, that grow towards
        /// <see cref="short.MinValue"/> and <see cref="short.MaxValue"/>, as the generator size increases. However,
        /// short generation can be customised using the builder methods on <see cref="IIntGen{short}"/>.
        /// <returns>The new generator.</returns>
        public static IIntGen<short> Int16() => new Int16Gen();
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
        /// <param name="maxExclusive">The maximum short to generate (exclusive).</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<short> LessThan(this IIntGen<short> gen, short maxExclusive) =>
            gen.LessThanEqual((short)(maxExclusive - 1));

        /// <summary>
        /// Constrains the generator so that it only produces values greater than the supplied minimum.
        /// </summary>
        /// <param name="minExclusive">The minimum integer to generate (exclusive).</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<short> GreaterThan(this IIntGen<short> gen, int minExclusive) =>
            gen.GreaterThanEqual((short)(minExclusive + 1));
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using System.Collections.Generic;

    internal class Int16Gen : BaseGen<short>, IIntGen<short>
    {
        private readonly Int16GenConfig _config;

        private record Int16GenConfig(
            short? Min,
            short? Max,
            short? Origin,
            Gen.Bias? Bias);

        private Int16Gen(Int16GenConfig config)
        {
            _config = config;
        }

        public Int16Gen() : this(new Int16GenConfig(Min: null, Max: null, Origin: null, Bias: null))
        {
        }

        public IIntGen<short> GreaterThanEqual(short min) => new Int16Gen(_config with { Min = min });

        public IIntGen<short> LessThanEqual(short max) => new Int16Gen(_config with { Max = max });

        public IIntGen<short> ShrinkTowards(short origin) => new Int16Gen(_config with { Origin = origin });

        public IIntGen<short> WithBias(Gen.Bias bias) => new Int16Gen(_config with { Bias = bias });

        protected override IEnumerable<IGenIteration<short>> Run(GenParameters parameters) =>
            CreateGen(_config).Advanced.Run(parameters);

        private static IGen<short> CreateGen(Int16GenConfig config)
        {
            var gen = Gen
                .Int32()
                .GreaterThanEqual(config.Min ?? short.MinValue)
                .LessThanEqual(config.Max ?? short.MaxValue);

            if (config.Origin != null)
            {
                gen = gen.ShrinkTowards(config.Origin.Value);
            }

            if (config.Bias != null)
            {
                gen = gen.WithBias(config.Bias.Value);
            }

            return gen
                .Select(x => (short)x)
                .SelectError(error => new GenErrorData(nameof(Int16Gen), error.Message));
        }
    }
}
