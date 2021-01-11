using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using GalaxyCheck.Internal.Sizing;
using System;
using System.Collections.Generic;

namespace GalaxyCheck
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
        /// Constrains the generator so that it only produces values between the supplied range (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IInt32Gen Between(int x, int y);

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

        protected override IEnumerable<GenIteration<int>> Run(IRng rng, Size size) =>
            BuildGen(_config).Advanced.Run(rng, size);

        private IInt32Gen WithPartialConfig(
            int? min = null, int? max = null, int? origin = null, Gen.Bias? bias = null)
        {
            return new Int32Gen(new IntegerGenConfig(
                Min: min ?? _config.Min,
                Max: max ?? _config.Max,
                Origin: origin ?? _config.Origin,
                Bias: bias ?? _config.Bias));
        }

        private IGen<int> BuildGen(IntegerGenConfig config)
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

            var getBounds = (config.Bias ?? Gen.Bias.Exponential) switch
            {
                Gen.Bias.None => BoundsScalingFactoryFuncs.Unscaled(min, max, origin),
                Gen.Bias.Linear => BoundsScalingFactoryFuncs.ScaledLinearly(min, max, origin),
                Gen.Bias.Exponential => BoundsScalingFactoryFuncs.ScaledExponentially(min, max, origin),
                _ => throw new NotSupportedException()
            };

            StatefulGenFunc<int> statefulGenFunc = (useNextInt, size) =>
            {
                var (min, max) = getBounds(size);
                return useNextInt(min, max);
            };

            return Gen.Advanced.Create(
                statefulGenFunc,
                ShrinkFunc.Towards(origin),
                MeasureFunc.DistanceFromOrigin(origin, min, max));
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
