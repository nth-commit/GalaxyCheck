namespace GalaxyCheck
{
    using GalaxyCheck.Gens;
    using System;

    public static partial class Gen
    {
        /// <summary>
        /// Creates a generator that produces 64-bit integers. By default, it will generate integers in the full range
        /// (<see cref="long.MinValue"/> to <see cref="long.MaxValue"/>), but the generator returned contains
        /// configuration methods to constrain the produced integers further.
        /// </summary>
        /// <returns>The new generator.</returns>
        public static IIntGen<long> Int64() => new Int64Gen(nameof(Int64));
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Constrains the generator so that it only produces values between the supplied range (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<long> Between(this IIntGen<long> gen, long x, long y)
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
        public static IIntGen<long> LessThan(this IIntGen<long> gen, long maxExclusive)
        {
            try
            {
                checked
                {
                    return gen.LessThanEqual((long)(maxExclusive - 1));
                }
            }
            catch (OverflowException ex)
            {
                return new FatalIntGen<long>($"{nameof(Gen.Int64)}().{nameof(LessThan)}({maxExclusive})", ex);
            }
        }

        /// <summary>
        /// Constrains the generator so that it only produces values greater than the supplied minimum.
        /// </summary>
        /// <param name="minExclusive">The minimum integer to generate (exclusive).</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IIntGen<long> GreaterThan(this IIntGen<long> gen, long minExclusive)
        {
            try
            {
                checked
                {
                    return gen.GreaterThanEqual((long)(minExclusive + 1));
                }
            }
            catch (OverflowException ex)
            {
                return new FatalIntGen<long>($"{nameof(Gen.Int64)}().{nameof(GreaterThan)}({minExclusive})", ex);
            }
        }
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Parameters;
    using GalaxyCheck.Gens.Parameters.Internal;
    using GalaxyCheck.ExampleSpaces;
    using System;

    internal record Int64Gen(
        string PublicGenName,
        long? Min = null,
        long? Max = null,
        long? Origin = null,
        Gen.Bias? Bias = null) : GenProvider<long>, IIntGen<long>
    {
        public IIntGen<long> GreaterThanEqual(long min) => this with { Min = min };

        public IIntGen<long> LessThanEqual(long max) => this with { Max = max };

        public IIntGen<long> ShrinkTowards(long origin) => this with { Origin = origin };

        public IIntGen<long> WithBias(Gen.Bias bias) => this with { Bias = bias };

        protected override IGen<long> Get
        {
            get
            {
                var min = Min ?? long.MinValue;
                var max = Max ?? long.MaxValue;

                if (min > max)
                {
                    return Error(PublicGenName, "'min' cannot be greater than 'max'");
                }

                if (Origin != null && (Origin < min || Origin > max))
                {
                    return Error(PublicGenName, "'origin' must be between 'min' and 'max'");
                }

                if (min == max)
                {
                    return Gen.Constant(min);
                }

                var origin = Origin ?? InferOrigin(min, max);
                var bias = Bias ?? Gen.Bias.WithSize;

                var genFunc = bias == Gen.Bias.WithSize
                    ? CreateBiasedStatefulGen(min, max, origin)
                    : CreateUnbiasedStatefulGen(min, max);

                return Gen
                    .Create(genFunc.Invoke)
                    .Unfold(value => ExampleSpaceFactory.Int64(value: value, origin: origin, min: min, max: max));
            }
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

        private static IGen<long> Error(string publicGenName, string message) => Gen.Advanced.Error<long>(publicGenName, message);

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
