﻿using GalaxyCheck.Abstractions;
using GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Gens
{
    public interface IIntegerGenBuilder : IGenBuilder<int>
    {
        /// <summary>
        /// Constrains the generator so that it only produces values less than or equal to the supplied maximum.
        /// </summary>
        /// <param name="max">The maximum integer to generate.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IIntegerGenBuilder LessThanEqual(int max);

        /// <summary>
        /// Constrains the generator so that it only produces values greater than or equal to the supplied minimum.
        /// </summary>
        /// <param name="min">The minimum integer to generate.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IIntegerGenBuilder GreaterThanEqual(int min);

        /// <summary>
        /// Constrains the generator so that it only produces values between the supplied range (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IIntegerGenBuilder Between(int x, int y);
    }

    public class IntegerGenBuilder : BaseGenBuilder<int>, IIntegerGenBuilder
    {
        public static IntegerGenBuilder Create() => new IntegerGenBuilder(new IntegerGenConfig(Min: null, Max: null));

        private record IntegerGenConfig(int? Min, int? Max)
        {
            public IGen<int> ToGen() => new DelayedGen<int>(() => PrimitiveGenBuilder.Create(
                (useNextInt) => useNextInt(Min ?? int.MinValue, Max ?? int.MaxValue),
                ShrinkFunc.Towards(0),
                MeasureFunc.Unmeasured<int>()));
        }

        private readonly IntegerGenConfig _config;

        private IntegerGenBuilder(IntegerGenConfig config) : base(config.ToGen())
        {
            _config = config;
        }

        public IIntegerGenBuilder LessThanEqual(int max) => WithPartialConfig(max: max);

        public IIntegerGenBuilder GreaterThanEqual(int min) => WithPartialConfig(min: min);

        public IIntegerGenBuilder Between(int x, int y) => WithPartialConfig(
            min: x > y ? y : x,
            max: x > y ? x : y);

        private IIntegerGenBuilder WithPartialConfig(int? min = null, int? max = null)
        {
            return new IntegerGenBuilder(new IntegerGenConfig(
                Min: min ?? _config.Min,
                Max: max ?? _config.Max));
        }
    }
}
