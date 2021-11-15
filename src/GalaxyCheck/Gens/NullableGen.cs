namespace GalaxyCheck
{
    public static partial class Gen
    {
        /// <summary>
        /// Generates values from the source generator or nulls, at a ratio of 10:1.
        /// </summary>
        /// <param name="gen">The generator to produce the non-nullable values.</param>
        /// <returns>A new generator.</returns>
        public static IGen<T?> Nullable<T>(IGen<T> gen)
            where T : class
        {
            return Choose<T?>()
                .WithChoice(Constant<T?>(default), 1)
                .WithChoice(gen, 10);
        }
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Generates values from the source generator or nulls, at a ratio of 10:1.
        /// </summary>
        /// <param name="gen">The generator to produce the non-nullable values.</param>
        /// <returns>A new generator.</returns>
        public static IGen<T?> OrNull<T>(this IGen<T> gen) where T : class => Gen.Nullable(gen);
    }
}
