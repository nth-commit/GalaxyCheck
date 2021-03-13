using System.Collections.Generic;

namespace GalaxyCheck.Internal.Replaying
{
    public record Replay(
        GenParameters GenParameters,
        IEnumerable<int> ExampleSpacePath);
}
