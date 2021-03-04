namespace GalaxyCheck
{
    public static class Symbols
    {
        /// <summary>
        /// A symbol indicating that a pair of values were bound together by a generator, rather than being
        /// intentionally paired by a consumer (e.g. if they were generating a tuple). The type can be
        /// recursively unwrapped to provide a simple presentation for the consumer.
        /// </summary>
        public record BoundTuple<TLeft, TRight>(TLeft Left, TRight Right);

        /// <summary>
        /// A symbol indicating an empty input to a property. The property might actually have some interesting values
        /// that aren't captured in the iteration itself (e.g. they could just be in scope in a LINQ statement). The
        /// type can be used as a signal to look back through the history of example spaces, to find interesting values
        /// that were in scope, in order to present them as counterexamples for the consumer.
        /// </summary>
        public record NoInput;
    }
}
