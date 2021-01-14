using GalaxyCheck.Gens;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using GalaxyCheck.Internal.Sizing;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        /// <summary>
        /// Creates a generator that produces lists, the elements of which are produced by the given generator. By
        /// default, the generator produces lists ranging from length 0 to 20 - but this can be configured using the
        /// builder methods on <see cref="IListGen{T}"/>.
        /// </summary>
        /// <param name="elementGen">The generator used to produce the elements of the list.</param>
        /// <returns>The new generator.</returns>
        public static IListGen<T> List<T>(IGen<T> elementGen) => new ListGen<T>(elementGen);
    }
}

namespace GalaxyCheck.Gens
{
    public interface IListGen<T> : IGen<ImmutableList<T>>
    {
        /// <summary>
        /// Constrains the generator so that it only produces lists with the given length.
        /// </summary>
        /// <param name="length">The length to constrain generated lists to.</param>
        /// <returns>A new generator with the constrain applied.</returns>
        IListGen<T> OfLength(int length);

        /// <summary>
        /// Constrains the generator so that it only produces lists with at least the given length.
        /// </summary>
        /// <param name="minLength">The minimum length that generated lists should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IListGen<T> OfMinimumLength(int minLength);

        /// <summary>
        /// Constrains the generator so that it only produces lists with at most the given length.
        /// </summary>
        /// <param name="maxLength">The maximum length that generated lists should be constrained to.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IListGen<T> OfMaximumLength(int maxLength);

        /// <summary>
        /// Constrains the generator so that it only produces lists with lengths within the supplied range (inclusive).
        /// </summary>
        /// <param name="x">The first bound of the range.</param>
        /// <param name="y">The second bound of the range.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IListGen<T> BetweenLengths(int x, int y);

        /// <summary>
        /// Modifies how the generator biases lengths of produced lists with respect to the size parameter.
        /// </summary>
        /// <returns>A new generator with the biasing effect applied.</returns>
        IListGen<T> WithLengthBias(Gen.Bias bias);
    }

    public class ListGen<T> : BaseGen<ImmutableList<T>>, IListGen<T>
    {
        private readonly IGen<T> _elementGen;

        public ListGen(IGen<T> elementGen)
        {
            _elementGen = elementGen;
        }

        public IListGen<T> OfLength(int length)
        {
            throw new System.NotImplementedException();
        }

        public IListGen<T> OfMinimumLength(int minLength)
        {
            throw new System.NotImplementedException();
        }

        public IListGen<T> OfMaximumLength(int maxLength)
        {
            throw new System.NotImplementedException();
        }

        public IListGen<T> BetweenLengths(int x, int y)
        {
            throw new System.NotImplementedException();
        }

        public IListGen<T> WithLengthBias(Gen.Bias bias)
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<GenIteration<ImmutableList<T>>> Run(IRng rng, Size size)
        {
            throw new System.NotImplementedException();
        }
    }
}