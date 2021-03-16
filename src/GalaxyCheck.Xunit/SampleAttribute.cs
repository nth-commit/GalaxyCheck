using System;
using Xunit;
using Xunit.Sdk;

namespace GalaxyCheck.Xunit
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("GalaxyCheck.Xunit.Internal.SampleDiscoverer", "GalaxyCheck.Xunit")]
    public class SampleAttribute : PropertyAttribute
    {
    }
}
