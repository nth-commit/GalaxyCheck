namespace GalaxyCheck
{
    public static partial class Gen
    {
        private const int NonNullFrequency = 5;

        /// <summary>
        /// Generates values from the source generator or nulls, at a ratio of 5:1.
        /// 
        /// Note; for generating nullable structs (e.g. `int?`), use <see cref="NullableStruct{T}(IGen{T})"/>. This is
        /// due to language limitations of C#.
        /// </summary>
        /// <param name="gen">The generator to produce the non-nullable values.</param>
        /// <returns>A new generator.</returns>
        public static IGen<T?> Nullable<T>(IGen<T> gen)
            where T : class
        {
            return Choose<T?>()
                .WithChoice(Constant<T?>(null), 1)
                .WithChoice(gen, NonNullFrequency);
        }

        /// <summary>
        /// Generates values from the source generator or nulls, at a ratio of 5:1.
        /// </summary>
        /// <param name="gen">The generator to produce the non-nullable values.</param>
        /// <returns>A new generator.</returns>
        public static IGen<T?> NullableStruct<T>(IGen<T> gen)
            where T : struct
        {
            return Choose<T?>()
                .WithChoice(Constant<T?>(null), 1)
                .WithChoice(gen.Cast<T?>(), NonNullFrequency);
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

        /// <summary>
        /// Generates values from the source generator or nulls, at a ratio of 10:1.
        /// 
        /// Note; for generating nullable structs (e.g. `int?`), use <see cref="NullableStruct{T}(IGen{T})"/>. This is
        /// due to language limitations of C#.
        /// </summary>
        /// <param name="gen">The generator to produce the non-nullable values.</param>
        /// <returns>A new generator.</returns>
        public static IGen<T?> OrNullStruct<T>(this IGen<T> gen) where T : struct => Gen.NullableStruct(gen);
    }
}
