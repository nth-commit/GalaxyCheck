using GalaxyCheck.Xunit.Internal;
using System;
using Xunit;
using Xunit.Sdk;

namespace GalaxyCheck.Xunit
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("GalaxyCheck.Xunit.Internal.PropertyDiscoverer", "GalaxyCheck.Xunit")]
    public class PropertyAttribute : FactAttribute
    {
        public int ShrinkLimit { get; set; } = 500;

        public int Iterations { get; set; } = 100;

        public virtual IPropertyRunner Runner => new PropertyAssertRunner();
    }
}
