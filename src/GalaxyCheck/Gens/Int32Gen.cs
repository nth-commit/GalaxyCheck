namespace GalaxyCheck
{
    using GalaxyCheck.Gens;

    public static partial class Gen
    {
        /// <summary>
        /// Generates 32-bit integers (ints). By default, the generator produces ints from 0, that grow towards
        /// <see cref="int.MinValue"/> and <see cref="int.MaxValue"/>, as the generator size increases. However,
        /// int generation can be customised using the builder methods on <see cref="IIntGen{int}"/>.
        /// <returns>The new generator.</returns>
        public static IIntGen<int> Int32() => new Int32Gen();
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
        public static IIntGen<int> LessThan(this IIntGen<int> gen, int maxExclusive) =>
            gen.LessThanEqual((int)(maxExclusive - 1));

        /// <summary>
        /// Constrains the generator so that it only produces values greater than the supplied minimum.
        /// </summary>
        /// <param name="minExclusive">The minimum integer to generate (exclusive).</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<int> GreaterThan(this IIntGen<int> gen, int minExclusive) =>
            gen.GreaterThanEqual((int)(minExclusive + 1));
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using System.Collections.Generic;

    internal class Int32Gen: BaseGen<int>, IIntGen<int>
    {
        private readonly Int32GenConfig _config;

        private record Int32GenConfig(
            int? Min,
            int? Max,
            int? Origin,
            Gen.Bias? Bias);

        private Int32Gen(Int32GenConfig config)
        {
            _config = config;
        }

        public Int32Gen() : this(new Int32GenConfig(Min: null, Max: null, Origin: null, Bias: null))
        {
        }

        public IIntGen<int> GreaterThanEqual(int min) => new Int32Gen(_config with { Min = min });

        public IIntGen<int> LessThanEqual(int max) => new Int32Gen(_config with { Max = max });

        public IIntGen<int> ShrinkTowards(int origin) => new Int32Gen(_config with { Origin = origin });

        public IIntGen<int> WithBias(Gen.Bias bias) => new Int32Gen(_config with { Bias = bias });

        protected override IEnumerable<IGenIteration<int>> Run(GenParameters parameters) =>
            CreateGen(_config).Advanced.Run(parameters);

        private static IGen<int> CreateGen(Int32GenConfig config)
        {
            var gen = Gen
                .Int64()
                .GreaterThanEqual(config.Min ?? int.MinValue)
                .LessThanEqual(config.Max ?? int.MaxValue);

            if (config.Origin != null)
            {
                gen = gen.ShrinkTowards(config.Origin.Value);
            }

            if (config.Bias != null)
            {
                gen = gen.WithBias(config.Bias.Value);
            }

            return gen
                .Select(x => (int)x)
                .SelectError(error => new GenErrorData(nameof(Int32Gen), error.Message));
        }
    }
}
