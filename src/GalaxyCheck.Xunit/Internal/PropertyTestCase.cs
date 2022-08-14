using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GalaxyCheck.Internal
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

        public override async Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            messageBus.QueueMessage(new TestCaseStarting(this));

            var startTime = DateTime.UtcNow;

            var test = new XunitTest(this, DisplayName);
            var testOutputHelper = constructorArguments
                .OfType<TestOutputHelper>()
                .FirstOrDefault() ?? new TestOutputHelper();
            testOutputHelper.Initialize(messageBus, test);

            RunSummary Fail(Exception ex)
            {
                var executionTime = (decimal)((DateTime.UtcNow - startTime).TotalSeconds);
                messageBus.QueueMessage(new TestFailed(test, executionTime, testOutputHelper!.Output, ex));
                return new RunSummary() { Failed = 1, Total = 1, Skipped = 0, Time = executionTime  };
            }

            RunSummary Pass()
            {
                var executionTime = (decimal)((DateTime.UtcNow - startTime).TotalSeconds);
                messageBus.QueueMessage(new TestPassed(test, executionTime, testOutputHelper!.Output));
                return new RunSummary() { Failed = 0, Total = 1, Skipped = 0, Time = executionTime };
            }

            RunSummary Skip(string reason)
            {
                var executionTime = (decimal)((DateTime.UtcNow - startTime).TotalSeconds);
                messageBus.QueueMessage(new TestSkipped(test, reason));
                return new RunSummary() { Failed = 0, Total = 0, Skipped = 1, Time = executionTime };
            }

            PropertyInitializationResult? propertyInitResult;
            try
            {
                propertyInitResult = PropertyInitializer.Initialize(
                    TestMethod.TestClass.Class.ToRuntimeType(),
                    TestMethod.Method.ToRuntimeMethod(),
                    constructorArguments,
                    new DefaultPropertyFactory(),
                    GlobalConfiguration.Instance.Properties);
            }
            catch (Exception exception)
            {
                return Fail(exception);
            }

            if (propertyInitResult.ShouldSkip)
            {
                return Skip(propertyInitResult.SkipReason!);
            }

            try
            {
                await propertyInitResult.Runner.Run(propertyInitResult.Parameters, testOutputHelper);
                return Pass();
            }
            catch (Exception propertyFailedException)
            {
                return Fail(propertyFailedException);
            }
        }
    }
}
