using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using GalaxyCheck.Internal.Sizing;
using GalaxyCheck.Internal.WeightedLists;
using System;
using System.Collections.Generic;

namespace GalaxyCheck.Gens
{
    public interface IInt32Gen : IGen<int>
    {
        /// <summary>
        /// Constrains the generator so that it only produces values less than or equal to the supplied maximum.
        /// </summary>
        /// <param name="max">The maximum integer to generate.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IInt32Gen LessThanEqual(int max);

        /// <summary>
        /// Constrains the generator so that it only produces values greater than or equal to the supplied minimum.
        /// </summary>
        /// <param name="min">The minimum integer to generate.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IInt32Gen GreaterThanEqual(int min);

        /// <summary>
        /// Modifies the generator so that values will ultimately shrink to the supplied origin. The origin is the
        /// "smallest" value that all values should shrink towards. By default, the origin will be 0. The origin must
        /// be within the the bounds of the generator. If the bounds have been modified so that they don't contain 0,
        /// and an origin has not been supplied, the generator will try and infer the most sensible origin.
        /// </summary>
        /// <param name="origin">The "smallest" value that generated integers should shrink towards.</param>
        /// <returns>A new generator with the origin applied.</returns>
        IInt32Gen ShrinkTowards(int origin);

        /// <summary>
        /// Modifies how the generator biases values with respect to the size parameter.
        /// </summary>
        /// <returns>A new generator with the biasing effect applied.</returns>
        IInt32Gen WithBias(Gen.Bias bias);
    }
}

namespace GalaxyCheck
{
    using GalaxyCheck.Gens;
    using GalaxyCheck.Gens.Int32;

    public static partial class Gen
    {
        /// <summary>
        /// Creates a generator that produces 32-bit integers. By default, it will generate integers in the full range
        /// (-2,147,483,648 to 2,147,483,647), but the generator returned contains configuration methods to constrain
        /// the produced integers further.
        /// </summary>
        /// <returns>The new generator.</returns>
        public static IInt32Gen Int32() => new Int32Gen();

        /// <summary>
        /// Constrains the generator so that it only produces values between the supplied range (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IInt32Gen Between(this IInt32Gen gen, int x, int y)
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
        public static IInt32Gen LessThan(this IInt32Gen gen, int maxExclusive) => gen.LessThanEqual(maxExclusive - 1);

        /// <summary>
        /// Constrains the generator so that it only produces values greater than the supplied minimum.
        /// </summary>
        /// <param name="minExclusive">The minimum integer to generate (exclusive).</param>
        /// <returns>A new generator with the constraint applied.</returns>
        public static IInt32Gen GreaterThan(this IInt32Gen gen, int minExclusive) => gen.GreaterThanEqual(minExclusive + 1);
    }
}

namespace GalaxyCheck.Gens.Int32
{
    public class Int32Gen : BaseGen<int>, IInt32Gen
    {
        private record IntegerGenConfig(int? Min, int? Max, int? Origin, Gen.Bias? Bias);

        private readonly IntegerGenConfig _config;

        private Int32Gen(IntegerGenConfig config)
        {
            _config = config;
        }

        public Int32Gen()
            : this(new IntegerGenConfig(Min: null, Max: null, Origin: null, Bias: null))
        {
        }

        public IInt32Gen LessThanEqual(int max) => WithPartialConfig(max: max);

        public IInt32Gen GreaterThanEqual(int min) => WithPartialConfig(min: min);

        public IInt32Gen Between(int x, int y) => WithPartialConfig(min: x > y ? y : x, max: x > y ? x : y);

        public IInt32Gen ShrinkTowards(int origin) => WithPartialConfig(origin: origin);

        public IInt32Gen WithBias(Gen.Bias bias) => WithPartialConfig(bias: bias);

        protected override IEnumerable<IGenIteration<int>> Run(GenParameters parameters) =>
            BuildGen(_config).Advanced.Run(parameters);

        private IInt32Gen WithPartialConfig(
            int? min = null, int? max = null, int? origin = null, Gen.Bias? bias = null)
        {
            return new Int32Gen(new IntegerGenConfig(
                Min: min ?? _config.Min,
                Max: max ?? _config.Max,
                Origin: origin ?? _config.Origin,
                Bias: bias ?? _config.Bias));
        }

        private static IGen<int> BuildGen(IntegerGenConfig config)
        {
            var min = config.Min ?? int.MinValue;
            var max = config.Max ?? int.MaxValue;

            if (min > max)
            {
                return Error("'min' cannot be greater than 'max'");
            }

            if (config.Origin != null && (config.Origin < min || config.Origin > max))
            {
                return Error("'origin' must be between 'min' and 'max'");
            }

            var origin = config.Origin ?? InferOrigin(min, max);
            var bias = config.Bias ?? Gen.Bias.WithSize;

            var statefulGen = bias == Gen.Bias.WithSize
                ? CreateBiasedStatefulGen(min, max, origin)
                : CreateUnbiasedStatefulGen(min, max);

            return Gen
                .Advanced.Create(statefulGen)
                .Advanced.Unfold(value => ExampleSpaceFactory.Int32(value: value, origin: origin, min: min, max: max));
        }

        private static StatefulGenFunc<int> CreateUnbiasedStatefulGen(int min, int max) =>
            (useNextInt, _) => useNextInt(min, max);

        private static StatefulGenFunc<int> CreateBiasedStatefulGen(int min, int max, int origin)
        {
            var sizeToBounds = SizingToBounds.Exponential(min, max, origin);
            int? extremeMinimum = min == origin ? null : min;
            int? extremeMaximum = max == origin ? null : max;

            StatefulGenFunc<int> inextreme = FromBounds(sizeToBounds);

            StatefulGenFunc<int> withExtreme1 = OrExtreme(inextreme, extremeMinimum, extremeMaximum, 16, 1);

            StatefulGenFunc<int> withExtreme2 = OrExtreme(inextreme, extremeMinimum, extremeMaximum, 8, 1);

            StatefulGenFunc<int> withExtreme3 = OrExtreme(inextreme, extremeMinimum, extremeMaximum, 4, 1);

            StatefulGenFunc<int> withExtreme4 = OrExtreme(inextreme, extremeMinimum, extremeMaximum, 2, 1);

            StatefulGenFunc<int> withExtreme5 = OrExtreme(inextreme, extremeMinimum, extremeMaximum, 1, 1);

            return (useNextInt, size) =>
            {
                var innerGenFunc = size.Value switch
                {
                    <= 50 => inextreme,
                    <= 60 => withExtreme1,
                    <= 70 => withExtreme2,
                    <= 80 => withExtreme3,
                    <= 90 => withExtreme4,
                    _ => withExtreme5
                };

                return innerGenFunc(useNextInt, size);
            };
        }

        private static StatefulGenFunc<int> FromBounds(SizeToBoundsFunc sizeToBounds) => (useNextInt, size) =>
        {
            var (min, max) = sizeToBounds(size);
            return useNextInt(min, max);
        };

        /// <summary>
        /// Creates a stateful generator function which favours the extreme of the generator by a given frequency.
        /// </summary>
        private static StatefulGenFunc<int> OrExtreme(
            StatefulGenFunc<int> inextremeGenerator,
            int? extremeMinimum,
            int? extremeMaximum,
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

            return (useNextInt, size) =>
            {
                var source = weightedList[useNextInt(0, weightedList.Count - 1)];
                return source switch
                {
                    BoundsOrExtremeSource.Bounds => inextremeGenerator(useNextInt, size),
                    BoundsOrExtremeSource.ExtremeMinimum => extremeMinimum!.Value,
                    BoundsOrExtremeSource.ExtremeMaximum => extremeMaximum!.Value,
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

        private static IGen<int> Error(string message) => new ErrorGen<int>(nameof(Int32Gen), message);

        /// <summary>
        /// Infer the origin by finding the closest value to 0 that is within the bounds.
        /// </summary>
        private static int InferOrigin(int min, int max)
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
    }
}
