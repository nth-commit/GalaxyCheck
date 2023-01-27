using GalaxyCheck.Gens.Internal;
using System;

namespace GalaxyCheck.Gens
{
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

    internal record FatalIntGen<T> : GenProvider<T>, IIntGen<T>
    {
        private readonly Exception _ex;

        public FatalIntGen(Exception ex)
        {
            _ex = ex;
        }

        public IIntGen<T> GreaterThanEqual(T min) => this;

        public IIntGen<T> LessThanEqual(T max) => this;

        public IIntGen<T> ShrinkTowards(T origin) => this;

        public IIntGen<T> WithBias(Gen.Bias bias) => this;

        protected override IGen<T> Get => Gen.Advanced.Error<T>(_ex.Message);
    }
}
