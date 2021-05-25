namespace GalaxyCheck
{
    public static partial class Extensions
    {
        /// <summary>
        /// <para>
        /// Generates a tuple of two elements, where the values of the elements are sourced from the given generator.
        /// </para>
        /// <para>
        /// <code>
        /// Gen.Int32().Two(); // (0, 0), (0, 1), (1, 4) ...
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="gen">The generator used to produce the elements of the tuple.</param>
        /// <returns>A new generator.</returns>
        public static IGen<(T, T)> Two<T>(this IGen<T> gen) =>
            from x0 in gen
            from x1 in gen
            select (x0, x1);

        /// <summary>
        /// <para>
        /// Generates a tuple of three elements, where the values of the elements are sourced from the given generator.
        /// </para>
        /// <para>
        /// <code>
        /// Gen.Int32().Three(); // (0, 0, 0), (0, 1, 2), (1, 4, 3) ...
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="gen">The generator used to produce the elements of the tuple.</param>
        /// <returns>A new generator.</returns>
        public static IGen<(T, T, T)> Three<T>(this IGen<T> gen) =>
            from x0 in gen
            from x1 in gen
            from x2 in gen
            select (x0, x1, x2);

        /// <summary>
        /// <para>
        /// Generates a tuple of four elements, where the values of the elements are sourced from the given generator.
        /// </para>
        /// <para>
        /// <code>
        /// Gen.Int32().Four(); // (0, 0, 0, 0), (0, 1, 2, 4), (1, 4, 3, 9) ...
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="gen">The generator used to produce the elements of the tuple.</param>
        /// <returns>A new generator.</returns>
        public static IGen<(T, T, T, T)> Four<T>(this IGen<T> gen) =>
            from x0 in gen
            from x1 in gen
            from x2 in gen
            from x3 in gen
            select (x0, x1, x2, x3);
    }
}
