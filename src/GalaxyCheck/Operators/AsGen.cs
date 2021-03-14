namespace GalaxyCheck
{
    public static partial class Extensions
    {
        public static IGen<T> AsGen<T>(this IGenAdvanced<T> advanced) => new AdvancedToGen<T>(advanced);

        private class AdvancedToGen<T> : IGen<T>
        {
            public AdvancedToGen(IGenAdvanced<T> advanced)
            {
                Advanced = advanced;
            }

            public IGenAdvanced<T> Advanced { get; }

            IGenAdvanced IGen.Advanced => Advanced;
        }
    }
}
