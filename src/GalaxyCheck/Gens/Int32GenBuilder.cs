using GalaxyCheck.Abstractions;
using GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Gens
{
    public interface IInt32GenBuilder : IGenBuilder<int>
    {
        /// <summary>
        /// Constrains the generator so that it only produces values less than or equal to the supplied maximum.
        /// </summary>
        /// <param name="max">The maximum integer to generate.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IInt32GenBuilder LessThanEqual(int max);

        /// <summary>
        /// Constrains the generator so that it only produces values greater than or equal to the supplied minimum.
        /// </summary>
        /// <param name="min">The minimum integer to generate.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IInt32GenBuilder GreaterThanEqual(int min);

        /// <summary>
        /// Constrains the generator so that it only produces values between the supplied range (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IInt32GenBuilder Between(int x, int y);

        /// <summary>
        /// Modifies the generator so that values will ultimately shrink to the supplied origin. The origin is the
        /// "smallest" value that all values should shrink towards. By default, the origin will be 0. The origin must
        /// be within the the bounds of the generator. If the bounds have been modified so that they don't contain 0,
        /// and an origin has not been supplied, the generator will try and infer the most sensible origin.
        /// </summary>
        /// <param name="origin">The "smallest" value that generated integers should shrink towards.</param>
        /// <returns>A new generator with the origin applied.</returns>
        IInt32GenBuilder ShrinkTowards(int origin);
    }

    public class Int32GenBuilder : BaseGenBuilder<int>, IInt32GenBuilder
    {
        public static Int32GenBuilder Create() => new Int32GenBuilder(new IntegerGenConfig(Min: null, Max: null, Origin: null));

        private readonly IntegerGenConfig _config;

        private Int32GenBuilder(IntegerGenConfig config) : base(config.ToGen())
        {
            _config = config;
        }

        public IInt32GenBuilder LessThanEqual(int max) => WithPartialConfig(max: max);

        public IInt32GenBuilder GreaterThanEqual(int min) => WithPartialConfig(min: min);

        public IInt32GenBuilder Between(int x, int y) => WithPartialConfig(
            min: x > y ? y : x,
            max: x > y ? x : y);

        public IInt32GenBuilder ShrinkTowards(int origin) => WithPartialConfig(origin: origin);

        private IInt32GenBuilder WithPartialConfig(int? min = null, int? max = null, int? origin = null)
        {
            return new Int32GenBuilder(new IntegerGenConfig(
                Min: min ?? _config.Min,
                Max: max ?? _config.Max,
                Origin: origin ?? _config.Origin));
        }

        private record IntegerGenConfig(int? Min, int? Max, int? Origin)
        {
            public IGen<int> ToGen() => new DelayedGen<int>(() =>
            {
                var min = Min ?? int.MinValue;
                var max = Max ?? int.MaxValue;

                if (min > max)
                {
                    return Error("'min' cannot be greater than 'max'");
                }

                if (Origin != null && (Origin < min || Origin > max))
                {
                    return Error("'origin' must be between 'min' and 'max'");
                }

                var origin = Origin ?? InferOrigin(min, max);

                return PrimitiveGenBuilder.Create(
                    (useNextInt) => useNextInt(min, max),
                    ShrinkFunc.Towards(origin),
                    MeasureFunc.Unmeasured<int>());
            });

            private static IGen<int> Error(string message) => new ErrorGen<int>(nameof(Int32GenBuilder), message);

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
}
