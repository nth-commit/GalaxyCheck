using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GalaxyCheck.Internal.Infrastructure.Xunit
{
    public class PropertyDiscoverer : TheoryDiscoverer
    {
        private readonly IMessageSink _messageSink;

        public PropertyDiscoverer(IMessageSink messageSink) : base(messageSink)
        {
            _messageSink = messageSink;
        }

        public override IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            if (testMethod.Method.GetCustomAttributes(typeof(DataAttribute)).Any())
            {
                foreach (var testCase in base.Discover(discoveryOptions, testMethod, factAttribute))
                {
                    yield return testCase;
                }
            }
            else
            {
                yield return new PropertyTestCase(
                    _messageSink,
                    discoveryOptions.MethodDisplayOrDefault(),
                    discoveryOptions.MethodDisplayOptionsOrDefault(),
                    testMethod,
                    null);
            }
        }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo theoryAttribute,
            object[] dataRow)
        {
            yield return new PropertyTestCase(
                _messageSink,
                discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(),
                testMethod,
                dataRow);
        }
    }
}
