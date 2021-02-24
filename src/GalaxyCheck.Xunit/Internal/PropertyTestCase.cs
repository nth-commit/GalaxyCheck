using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
            object[]? testMethodArguments = null)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
        {
        }

        public override Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            messageBus.QueueMessage(new TestCaseStarting(this));

            var summary = new RunSummary();

            var test = new XunitTest(this, DisplayName);

            foreach (var constructorArgument in constructorArguments)
            {
                if (constructorArgument is TestOutputHelper testOutputHelper)
                {
                    testOutputHelper.Initialize(messageBus, test);
                }
            }

            var methodInfo = TestMethod.Method.ToRuntimeMethod();
            if (methodInfo.ReturnType != typeof(void))
            {
                aggregator.Add(new NotImplementedException("Non-void properties are not yet implemented"));
                return Task.FromResult(summary);
            }

            IMessageSinkMessage resultMessage = RunProperty(TestMethod.TestClass.Class.ToRuntimeType(), methodInfo, constructorArguments) switch
            {
                Exception ex => new TestFailed(test, 0, "", ex),
                null => new TestPassed(test, 0, ""),
            };

            messageBus.QueueMessage(resultMessage);

            return Task.FromResult(summary);
        }

        private static Exception? RunProperty(
            Type testClass,
            MethodInfo testMethod,
            object[] constructorArguments)
        {
            var testInstance = Activator.CreateInstance(testClass, constructorArguments);
            try
            {
                testMethod.Assert(
                    testInstance,
                    formatValue: x => x.Length switch
                    {
                        0 => "(no value)",
                        1 => JsonSerializer.Serialize(x[0]),
                        _ => JsonSerializer.Serialize(x)
                    },
                    formatReproduction: (x) =>
                    {
                        var attributes =
                            new List<(string name, string value)>
                            {
                                ("Seed", x.seed.ToString(CultureInfo.InvariantCulture)),
                                ("Size", x.size.ToString(CultureInfo.InvariantCulture)),
                            }
                            .Select(x => $"{x.name} = {x.value}");

                        return $"({ string.Join(", ", attributes) })";
                    });
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}
