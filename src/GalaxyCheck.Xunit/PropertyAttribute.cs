using System;
using Xunit;
using Xunit.Sdk;

namespace GalaxyCheck.Xunit
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("GalaxyCheck.Xunit.Internal.PropertyDiscoverer", "GalaxyCheck.Xunit")]
    public class PropertyAttribute : FactAttribute
    {
    }
}
