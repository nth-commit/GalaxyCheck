using System;

namespace GalaxyCheck
{
    public partial class Property
    {
#pragma warning disable IDE1006 // Naming Styles
        public interface TestOutput
#pragma warning restore IDE1006 // Naming Styles
        {
            TestResult Result { get; }

            Exception? Exception { get; }
        }
    }
}
