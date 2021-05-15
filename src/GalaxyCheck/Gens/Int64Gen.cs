namespace GalaxyCheck
{
    using GalaxyCheck.Gens;

    public static partial class Gen
    {
        /// <summary>
        /// Creates a generator that produces 32-bit integers. By default, it will generate integers in the full range
        /// (<see cref="long.MinValue"/> to <see cref="long.MaxValue"/>), but the generator returned contains
        /// configuration methods to constrain the produced integers further.
        /// </summary>
        /// <returns>The new generator.</returns>
        public static IIntGen<long> Int64() => new Int64Gen();
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Constrains the generator so that it only produces values between the supplied range (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<long> Between(this IIntGen<long> gen, int x, int y)
        {
            var min = x > y ? y : x;
            var max = x > y ? x : y;
            return gen.GreaterThanEqual(min).LessThanEqual(max);
        }

        /// <summary>
        /// Constrains the generator so that it only produces values less than the supplied maximum. 
        /// </summary>
        /// <param name="maxExclusive">The maximum integer to generate (exclusive).</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<long> LessThan(this IIntGen<long> gen, int maxExclusive) => gen.LessThanEqual(maxExclusive - 1);

        /// <summary>
        /// Constrains the generator so that it only produces values greater than the supplied minimum.
        /// </summary>
        /// <param name="minExclusive">The minimum integer to generate (exclusive).</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<long> GreaterThan(this IIntGen<long> gen, int minExclusive) => gen.GreaterThanEqual(minExclusive + 1);
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using GalaxyCheck.ExampleSpaces;
    using System;
    using System.Collections.Generic;
    using static GalaxyCheck.Gen;
    using GalaxyCheck.Gens.Parameters.Internal;

    internal class Int64Gen : BaseGen<long>, IIntGen<long>
    {
        private record IntegerGenConfig(long? Min, long? Max, long? Origin, Gen.Bias? Bias);

        private readonly IntegerGenConfig _config;

        private Int64Gen(IntegerGenConfig config)
        {
            _config = config;
        }

        public Int64Gen()
            : this(new IntegerGenConfig(Min: null, Max: null, Origin: null, Bias: null))
        {
        }

        public IIntGen<long> LessThanEqual(long max) => WithPartialConfig(max: max);

        public IIntGen<long> GreaterThanEqual(long min) => WithPartialConfig(min: min);

        public IIntGen<long> Between(long x, long y) => WithPartialConfig(min: x > y ? y : x, max: x > y ? x : y);

        public IIntGen<long> ShrinkTowards(long origin) => WithPartialConfig(origin: origin);

        public IIntGen<long> WithBias(Gen.Bias bias) => WithPartialConfig(bias: bias);

        protected override IEnumerable<IGenIteration<long>> Run(GenParameters parameters) =>
            BuildGen(_config).Advanced.Run(parameters);

        private IIntGen<long> WithPartialConfig(
            long? min = null, long? max = null, long? origin = null, Gen.Bias? bias = null)
        {
            return new Int64Gen(new IntegerGenConfig(
                Min: min ?? _config.Min,
                Max: max ?? _config.Max,
                Origin: origin ?? _config.Origin,
                Bias: bias ?? _config.Bias));
        }

        private static IGen<long> BuildGen(IntegerGenConfig config)
        {
            var min = config.Min ?? long.MinValue;
            var max = config.Max ?? long.MaxValue;

            if (min > max)
            {
                return Error("'min' cannot be greater than 'max'");
            }

            if (config.Origin != null && (config.Origin < min || config.Origin > max))
            {
                return Error("'origin' must be between 'min' and 'max'");
            }

            var origin = config.Origin ?? InferOrigin(min, max);
            var bias = config.Bias ?? Bias.WithSize;

            var genFunc = bias == Bias.WithSize
                ? CreateBiasedStatefulGen(min, max, origin)
                : CreateUnbiasedStatefulGen(min, max);

            return Create(genFunc.Invoke).Unfold(value =>
                ExampleSpaceFactory.Int64(value: value, origin: origin, min: min, max: max));
        }

        private static Int64GenFunc CreateUnbiasedStatefulGen(long min, long max) => (parameters) => ConsumeNextLong(parameters, min, max);

        private static Int64GenFunc CreateBiasedStatefulGen(long min, long max, long origin)
        {
            var sizeToBounds = SizingToBoundsLong.Exponential(min, max, origin);
            long? extremeMinimum = min == origin ? null : min;
            long? extremeMaximum = max == origin ? null : max;
            Int64GenFunc inextreme = FromBounds(sizeToBounds);

            Int64GenFunc CreateExtremeGenFunc(int inextremeFrequency, int extremeFrequency) =>
                OrExtreme(inextreme, extremeMinimum, extremeMaximum, inextremeFrequency, extremeFrequency);

            Int64GenFunc withExtreme1 = CreateExtremeGenFunc(16, 1);
            Int64GenFunc withExtreme2 = CreateExtremeGenFunc(8, 1);
            Int64GenFunc withExtreme3 = CreateExtremeGenFunc(4, 1);
            Int64GenFunc withExtreme4 = CreateExtremeGenFunc(2, 1);
            Int64GenFunc withExtreme5 = CreateExtremeGenFunc(1, 1);

            Int64GenFunc genFunc = (parameters) =>
            {
                var innerGenFunc = parameters.Size.Value switch
                {
                    <= 50 => inextreme,
                    <= 60 => withExtreme1,
                    <= 70 => withExtreme2,
                    <= 80 => withExtreme3,
                    <= 90 => withExtreme4,
                    _ => withExtreme5
                };

                return innerGenFunc(parameters);
            };

            return genFunc;
        }

        private static Int64GenFunc FromBounds(SizeToBoundsLongFunc sizeToBounds) => (parameters) =>
        {
            var (min, max) = sizeToBounds(parameters.Size);
            return ConsumeNextLong(parameters, min, max);
        };

        /// <summary>
        /// Creates a stateful generator function which favours the extreme of the generator by a given frequency.
        /// </summary>
        private static Int64GenFunc OrExtreme(
            Int64GenFunc inextremeGenerator,
            long? extremeMinimum,
            long? extremeMaximum,
            int inextremeFrequency,
            int extremeFrequency)
        {
            if (extremeMinimum == null && extremeMaximum == null)
            {
                return inextremeGenerator;
            }

            // If both are extremes then we will pick from both. If only one is an extreme then we will double that
            // extreme's frequency.
            var weightedElements = new[]
            {
                new WeightedElement<BoundsOrExtremeSource>(inextremeFrequency * 2, BoundsOrExtremeSource.Bounds),
                new WeightedElement<BoundsOrExtremeSource>(
                    extremeFrequency,
                    extremeMinimum.HasValue ? BoundsOrExtremeSource.ExtremeMinimum : BoundsOrExtremeSource.ExtremeMaximum),
                new WeightedElement<BoundsOrExtremeSource>(
                    extremeFrequency,
                    extremeMaximum.HasValue ? BoundsOrExtremeSource.ExtremeMaximum : BoundsOrExtremeSource.ExtremeMinimum),
            };

            var weightedList = new WeightedList<BoundsOrExtremeSource>(weightedElements);

            return (parameters) =>
            {
                var (weightedIndex, parameters0) = ConsumeNextLong(parameters, 0, weightedList.Count - 1);
                var source = weightedList[(int)weightedIndex];
                return source switch
                {
                    BoundsOrExtremeSource.Bounds => inextremeGenerator(parameters0),
                    BoundsOrExtremeSource.ExtremeMinimum => (extremeMinimum!.Value, parameters0),
                    BoundsOrExtremeSource.ExtremeMaximum => (extremeMaximum!.Value, parameters0),
                    _ => throw new NotSupportedException("Fatal: Unhandled switch")
                };
            };
        }

        private enum BoundsOrExtremeSource
        {
            Bounds = 0,
            ExtremeMinimum = 1,
            ExtremeMaximum = 2
        }

        private static IGen<long> Error(string message) => Gen.Advanced.Error<long>(nameof(Int64Gen), message);

        /// <summary>
        /// Infer the origin by finding the closest value to 0 that is within the bounds.
        /// </summary>
        private static long InferOrigin(long min, long max)
        {
            if (min > 0)
            {
                // If the min is greater than 0, then it must be the closer value to 0 out of min/max
                return min;
            }

            if (max < 0)
            {
                // If the max is less than 0, then it must be the closer value to 0 out of min/max
                return max;
            }

            // Else, 0 definitely is within the bounds of min/max.
            return 0;
        }

        public static (long, GenParameters) ConsumeNextLong(GenParameters parameters, long min, long max)
        {
            var value = parameters.Rng.Value(min, max);
            var nextRng = parameters.Rng.Next();
            return (value, parameters with { Rng = nextRng });
        }

        private delegate (long value, GenParameters nextParameters) Int64GenFunc(GenParameters parameters);
    }
}
