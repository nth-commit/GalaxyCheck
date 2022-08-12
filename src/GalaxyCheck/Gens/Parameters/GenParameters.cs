namespace GalaxyCheck.Gens.Parameters
{
    public record GenParameters(
        IRng Rng,
        Size Size,
        IRng? RngWaypoint)
    {
        public static GenParameters Parse(int seed, int size, int? seedWaypoint = null)
        {
            return new GenParameters(
                Internal.Rng.Create(seed),
                new Size(size),
                seedWaypoint == null ? null : Internal.Rng.Create(seedWaypoint.Value));
        }

        internal static GenParameters Create(IRng rng, Size size)
        {
            return new GenParameters(rng, size, null);
        }

        internal static GenParameters CreateRandom(Size size)
        {
            return new GenParameters(Internal.Rng.Spawn(), size, null);
        }

        public override string ToString()
        {
            var result = $"Rng={Rng},Size={Size}";

            if (RngWaypoint != null)
            {
                result += $",RngWaypoing={RngWaypoint}";
            }

            return result;
        }
    }
}
