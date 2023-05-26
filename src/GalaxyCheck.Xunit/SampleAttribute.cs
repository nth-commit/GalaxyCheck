using GalaxyCheck.Internal;
using System;
using Xunit.Sdk;

namespace GalaxyCheck
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("GalaxyCheck.Internal.Infrastructure.Xunit.PropertyDiscoverer", "GalaxyCheck.Xunit")]
    public class SampleAttribute : PropertyAttribute
    {
        internal override IPropertyRunner Runner => new PropertySampleRunner();
    }
}
