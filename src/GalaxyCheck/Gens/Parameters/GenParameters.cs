namespace GalaxyCheck.Gens.Parameters
{
    public record GenParameters
    {
        public IRng Rng { get; init; }

        public Size Size { get; init; }

        private GenParameters(IRng rng, Size size)
        {
            Rng = rng;
            Size = size;
        }

        public static GenParameters Create(int seed, int size)
        {
            return new GenParameters(Internal.Rng.Create(seed), new Size(size));
        }

        public static GenParameters Create(int size)
        {
            return new GenParameters(Internal.Rng.Spawn(), new Size(size));
        }

        internal static GenParameters Create(IRng rng, Size size)
        {
            return new GenParameters(rng, size);
        }

        internal static GenParameters Create(Size size)
        {
            return new GenParameters(Internal.Rng.Spawn(), size);
        }

        public GenParameters With(
            IRng? rng = null,
            Size? size = null) =>
                new GenParameters(rng ?? Rng, size ?? Size);
    }
}
