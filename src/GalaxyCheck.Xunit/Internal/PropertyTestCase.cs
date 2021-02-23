using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GalaxyCheck.Xunit.Internal
{
    public class PropertyTestCase : XunitTestCase
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public PropertyTestCase()
        {
        }

        public PropertyTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            TestMethodDisplayOptions defaultMethodDisplayOptions,
            ITestMethod testMethod,
            object[] testMethodArguments = null)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
        {
        }
    }
}
