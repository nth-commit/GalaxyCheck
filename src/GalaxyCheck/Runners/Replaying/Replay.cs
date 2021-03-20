using GalaxyCheck.Gens.Parameters;
using System.Collections.Generic;

namespace GalaxyCheck.Runners.Replaying
{
    internal record Replay(
        GenParameters GenParameters,
        IEnumerable<int> ExampleSpacePath);
}
