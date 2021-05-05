using GalaxyCheck.Xunit.Internal;
using System;
using Xunit;
using Xunit.Sdk;

namespace GalaxyCheck
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("GalaxyCheck.Xunit.Internal.PropertyDiscoverer", "GalaxyCheck.Xunit")]
    public class PropertyAttribute : FactAttribute
    {
        public int ShrinkLimit { get; set; } = 500;

        public int Iterations { get; set; } = 100;

        public Type? Factory { get; set; } = null;

        internal virtual IPropertyRunner Runner => new PropertyAssertRunner();
    }
}
