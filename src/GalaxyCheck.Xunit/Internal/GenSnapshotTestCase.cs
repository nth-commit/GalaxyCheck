using GalaxyCheck;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GalaxyCheck.Internal
{
    public class GenSnapshotTestCase : XunitTestCase
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public GenSnapshotTestCase()
        {
        }

        public GenSnapshotTestCase(
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
            EnsureInitialized();

            messageBus.QueueMessage(new TestCaseStarting(this));

            var startTime = DateTime.UtcNow;
            var test = new XunitTest(this, DisplayName);
            var testOutputHelper = constructorArguments
                .OfType<TestOutputHelper>()
                .FirstOrDefault() ?? new TestOutputHelper();
            testOutputHelper.Initialize(messageBus, test);

            Task<RunSummary> Fail(Exception ex)
            {
                var executionTime = (decimal)(DateTime.UtcNow - startTime).TotalSeconds;
                messageBus.QueueMessage(new TestFailed(test, executionTime, testOutputHelper!.Output, ex));
                return Task.FromResult(new RunSummary() { Total = 0, Failed = 1 });
            }

            var testClassType = TestMethod.TestClass.Class.ToRuntimeType();
            var testMethod = TestMethod.Method.ToRuntimeMethod();

            GenSnapshotInitializationResult? initialization;
            try
            {
                initialization = GenSnapshotInitializer.Initialize(
                    testClassType,
                    testMethod,
                    constructorArguments,
                    new DefaultParametersGenFactory(),
                    GlobalConfiguration.Instance.GenSnapshots);
            }
            catch (Exception exception)
            {
                return await Fail(exception);
            }

            if (initialization.ShouldSkip)
            {
                messageBus.QueueMessage(new TestSkipped(test, initialization.SkipReason));
                return new RunSummary() { Skipped = initialization.Seeds.Count };
            }

            var results = await Task.WhenAll(initialization.Seeds
                .Select(async (_, seedIndex) =>
                {
                    var (exception, executionTime) = await GenSnapshotRunner.Run(initialization, testClassType, testMethod, seedIndex);

                    if (exception == null)
                    {
                        messageBus.QueueMessage(new TestPassed(test, executionTime, testOutputHelper!.Output));
                        return true;
                    }
                    else
                    {
                        messageBus.QueueMessage(new TestFailed(test, executionTime, testOutputHelper!.Output, exception));
                        return false;
                    }
                })
                .ToList());

            // TODO: Properly measure execution time
            var failures = results.Count(result => result == false);
            return new RunSummary() { Total = results.Count(), Failed = failures };
        }
    }
}
