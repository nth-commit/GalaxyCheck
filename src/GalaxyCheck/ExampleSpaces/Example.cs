namespace GalaxyCheck.ExampleSpaces
{
    /// <summary>
    /// An example that lives inside an example space.
    /// </summary>
    public interface IExample
    {
        ExampleId Id { get; }

        /// <summary>
        /// The example value.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// A metric which indicates how far the value is away from the smallest possible value. The metric is
        /// originally a proportion out of 100, but it composes when example spaces are composed. Therefore, it's
        /// possible for the distance metric to be arbitrarily high.
        /// </summary>
        decimal Distance { get; }
    }

    /// <summary>
    /// An example that lives inside an example space.
    /// </summary>
    /// <typeparam name="T">The type of the example's value.</typeparam>
    public interface IExample<out T> : IExample
    {
        /// <summary>
        /// The example value.
        /// </summary>
        new T Value { get; }
    }

    /// <inheritdoc/>
    public record Example<T> : IExample<T>
    {
        public ExampleId Id { get; init; }

        /// <inheritdoc/>
        public T Value { get; init; }

        /// <inheritdoc/>
        public decimal Distance { get; init; }

        public Example(ExampleId id, T value, decimal distance)
        {
            Id = id;
            Value = value;
            Distance = distance;
        }

        object IExample.Value => Value!;
    }
}
