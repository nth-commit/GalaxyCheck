using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace GalaxyCheck.Internal.Infrastructure.Xunit
{
    public class PropertyTestRunner : TestCaseRunner<PropertyTestCase>
    {
        private readonly object[] _constructorArguments;
        private readonly object[] _testMethodArguments;

        public PropertyTestRunner(
            PropertyTestCase testCase,
            IMessageBus messageBus,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource,
            object[] constructorArguments,
            object[] testMethodArguments)
            : base(testCase, messageBus, aggregator, cancellationTokenSource)
        {
            _constructorArguments = constructorArguments;
            _testMethodArguments = testMethodArguments;
        }

        protected override async Task<RunSummary> RunTestAsync()
        {
            var startTime = DateTime.UtcNow;

            var test = new XunitTest(TestCase, TestCase.DisplayName);
            var testOutputHelper = _constructorArguments
                .OfType<TestOutputHelper>()
                .FirstOrDefault() ?? new TestOutputHelper();
            testOutputHelper.Initialize(MessageBus, test);

            RunSummary Fail(Exception ex)
            {
                var executionTime = (decimal)((DateTime.UtcNow - startTime).TotalSeconds);
                MessageBus.QueueMessage(new TestFailed(test, executionTime, testOutputHelper!.Output, ex));
                return new RunSummary() { Failed = 1, Total = 1, Skipped = 0, Time = executionTime };
            }

            RunSummary Pass()
            {
                var executionTime = (decimal)((DateTime.UtcNow - startTime).TotalSeconds);
                MessageBus.QueueMessage(new TestPassed(test, executionTime, testOutputHelper!.Output));
                return new RunSummary() { Failed = 0, Total = 1, Skipped = 0, Time = executionTime };
            }

            RunSummary Skip(string reason)
            {
                var executionTime = (decimal)((DateTime.UtcNow - startTime).TotalSeconds);
                MessageBus.QueueMessage(new TestSkipped(test, reason));
                return new RunSummary() { Failed = 0, Total = 0, Skipped = 1, Time = executionTime };
            }

            PropertyInitializationResult? propertyInitResult;
            try
            {
                propertyInitResult = PropertyInitializer.Initialize(
                    TestCase.TestMethod.TestClass.Class.ToRuntimeType(),
                    TestCase.TestMethod.Method.ToRuntimeMethod(),
                    _constructorArguments,
                    _testMethodArguments,
                    new DefaultPropertyFactory(),
                    GlobalConfiguration.GetInstance(TestCase.TestMethod.TestClass.Class.ToRuntimeType().Assembly).Properties);
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
