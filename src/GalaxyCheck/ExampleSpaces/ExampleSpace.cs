using System.Collections.Generic;

namespace GalaxyCheck.ExampleSpaces
{
    /// <summary>
    /// Represents an space of examples. An example space generally has an original or root value, which can be
    /// explored recursively, and the space itself is a tree-like structure to enable efficient exploration of the
    /// example space.
    /// </summary>
    public interface IExampleSpace
    {
        IExample Current { get; }

        IEnumerable<IExampleSpace> Subspace { get; }
    }

    /// <inheritdoc/>
    public interface IExampleSpace<out T> : IExampleSpace
    {
        new IExample<T> Current { get; }

        new IEnumerable<IExampleSpace<T>> Subspace { get; }
    }
}
