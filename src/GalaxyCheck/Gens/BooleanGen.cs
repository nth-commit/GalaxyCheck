namespace GalaxyCheck
{
    public static partial class Gen
    {
        /// <summary>
        /// Generates booleans.
        /// <returns>The new generator.</returns>
        public static IGen<bool> Boolean() => Int32().Between(0, 1).Select(x => x == 1);
    }
}
