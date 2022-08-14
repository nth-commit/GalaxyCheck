using GalaxyCheck.Configuration;
using System;
using System.Threading.Tasks;

namespace GalaxyCheck.Internal
{
    internal class GlobalGenSnapshotConfiguration : IGlobalGenSnapshotConfiguration
    {
        public int DefaultIterations { get; set; } = 3;

        public int DefaultSize { get; set; } = 100;

        public Func<ISnapshot, Task>? AssertSnapshotMatches { get; set; } = null;
    }
}
