using System;
using System.Threading.Tasks;

namespace GalaxyCheck.Configuration
{
    public interface IGlobalConfiguration
    {
        IGlobalPropertyConfiguration Properties { get; }

        IGlobalGenSnapshotConfiguration GenSnapshots { get; }
    }

    public interface IGlobalPropertyConfiguration
    {
        int DefaultIterations { get; set; }

        int DefaultShrinkLimit { get; set; }
    }

    public interface IGlobalGenSnapshotConfiguration
    {
        int DefaultIterations { get; set; }

        int DefaultSize { get; set; }

        Func<ISnapshot, Task>? AssertSnapshotMatches { get; set; }
    }

    public interface ISnapshot
    {
        object?[] Input { get; }

        object? Output { get; }

        string TestFileName { get; }

        string TestClassName { get; }

        string TestMethodName { get; }

        int Seed { get; }
    }
}
