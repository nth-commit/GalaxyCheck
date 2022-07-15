using System;

namespace GalaxyCheck
{
#pragma warning disable IDE1006 // Naming Styles
    public interface TestOutput
#pragma warning restore IDE1006 // Naming Styles
    {
        TestResult Result { get; }

        Exception? Exception { get; }
    }
}
