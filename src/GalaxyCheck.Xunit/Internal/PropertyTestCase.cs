using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

            var startTime = DateTime.UtcNow;
            var test = new XunitTest(this, DisplayName);
            var testOutputHelper = constructorArguments
                .OfType<TestOutputHelper>()
                .FirstOrDefault() ?? new TestOutputHelper();
            testOutputHelper.Initialize(messageBus, test);

            Task<RunSummary> Fail(Exception ex)
            {
                var executionTime = (decimal)((DateTime.UtcNow - startTime).TotalSeconds);
                messageBus.QueueMessage(new TestFailed(test, executionTime, testOutputHelper!.Output, ex));
                return Task.FromResult(new RunSummary() { Failed = 1 });
            }

            Task<RunSummary> Pass()
            {
                var executionTime = (decimal)((DateTime.UtcNow - startTime).TotalSeconds);
                messageBus.QueueMessage(new TestPassed(test, executionTime, testOutputHelper!.Output));
                return Task.FromResult(new RunSummary() { Total = 1 });
            }

            var methodInfo = TestMethod.Method.ToRuntimeMethod();

            var methodInfoValidationException = ValidateMethod(methodInfo);
            if (methodInfoValidationException != null)
            {
                return Fail(methodInfoValidationException);
            }

            var testClass = TestMethod.TestClass.Class.ToRuntimeType();
            var testInstance = Activator.CreateInstance(testClass, constructorArguments);
            var property = methodInfo.ToProperty(testInstance);

            try
            {
                RunProperty(property, testOutputHelper);
                return Pass();
            }
            catch (Exception propertyFailedException)
            {
                return Fail(propertyFailedException);
            }
        }

        private static Exception? ValidateMethod(MethodInfo methodInfo)
        {
            var supportedReturnTypes = new List<Type>
            {
                typeof(void),
                typeof(bool),
                typeof(Property)
            };

            if (!supportedReturnTypes.Contains(methodInfo.ReturnType))
            {
                var supportedReturnTypesFormatted = string.Join(", ", supportedReturnTypes);
                return new Exception(
                    $"Return type {methodInfo.ReturnType} is not supported by GalaxyCheck.Xunit. Please use one of: {supportedReturnTypesFormatted}");
            }

            return null;
        }

        protected virtual void RunProperty(Property<object[]> property, ITestOutputHelper testOutputHelper)
        {
            property.Assert(
                formatValue: x => JsonSerializer.Serialize(x),
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
        }
    }
}