namespace GalaxyCheck.Internal.ExampleSpaces
{
    /// <summary>
    /// An example that lives inside an example space.
    /// </summary>
    /// <typeparam name="T">The type of the example's value.</typeparam>
    public record Example<T>
    {
        public ExampleId Id { get; init; }

        /// <summary>
        /// The example value.
        /// </summary>
        public T Value { get; init; }

        /// <summary>
        /// A metric which indicates how far the value is away from the smallest possible value. The metric is
        /// originally a proportion out of 100, but it composes when example spaces are composed. Therefore, it's
        /// possible for the distance metric to be arbitrarily high.
        /// </summary>
        public decimal Distance { get; init; }

        public Example(ExampleId id, T value, decimal distance)
        {
            Id = id;
            Value = value;
            Distance = distance;
        }
    }
}
