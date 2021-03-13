using System.Collections.Generic;

namespace GalaxyCheck.Internal.Replaying
{
    public record Replay(
        int Seed,
        int Size,
        IEnumerable<int> ExampleSpacePath);
}
