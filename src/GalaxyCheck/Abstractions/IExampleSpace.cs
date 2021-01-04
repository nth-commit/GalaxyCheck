using System.Collections.Generic;

namespace GalaxyCheck.Abstractions
{
    public interface IExampleSpace<T>
    {
        IEnumerable<LocatedExample<T>> Traverse();

        Example<T>? Minimal();
    }

    /// <summary>
    /// An example that lives inside an example space.
    /// </summary>
    /// <typeparam name="T">The type of the example's value.</typeparam>
    public record Example<T>
    {
        /// <summary>
        /// The example value.
        /// </summary>
        public T Value { get; init; }

        /// <summary>
        /// A metric which indicates how far the value is away from the smallest possible value. The metric is
        /// originally a proportion out of 100, but it composes when example spaces are composed. Therefore, it's
        /// possible for the distance metric to be arbitrarily high.
        /// </summary>
        public int Distance { get; init; }

        public Example(T value, int distance)
        {
            Value = value;
            Distance = distance;
        }
    }

    /// <summary>
    /// An example that has been located through traversal.
    /// </summary>
    /// <typeparam name="T">The type of the example's value.</typeparam>
    public record LocatedExample<T> : Example<T>
    {
        public int LevelIndex { get; init; }

        public int ChildIndex { get; init; }

        public LocatedExample(
            T value,
            int distance,
            int levelIndex,
            int childIndex)
            : base(value, distance)
        {
            LevelIndex = levelIndex;
            ChildIndex = childIndex;
        }
    }
}
