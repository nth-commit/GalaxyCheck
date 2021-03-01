using System;
using System.ComponentModel;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GalaxyCheck.Xunit.Internal
{
    public class SampleTestCase : PropertyTestCase
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public SampleTestCase()
        {
        }

        public SampleTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            TestMethodDisplayOptions defaultMethodDisplayOptions,
            ITestMethod testMethod,
            object[]? testMethodArguments = null)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
        {
        }

        protected override void RunProperty(Property<object[]> property, ITestOutputHelper testOutputHelper)
        {
            throw new SampleException(property.Select(x => x.Input).Advanced.SampleWithMetrics());
        }
    }
}