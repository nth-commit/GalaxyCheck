using GalaxyCheck.Internal.Sizing;

namespace GalaxyCheck.Gens.Parameters
{
    public record GenParameters(IRng Rng, Size Size)
    {
        public GenParameters With(
            IRng? rng = null,
            Size? size = null) =>
                new GenParameters(rng ?? Rng, size ?? Size);
    }
}
