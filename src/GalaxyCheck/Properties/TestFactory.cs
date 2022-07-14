using System;
using System.Threading.Tasks;

namespace GalaxyCheck.Properties
{
    internal static class TestFactory
    {
        private record TestOutputImpl(TestResult Result, Exception? Exception) : TestOutput;

        private record TestImpl<T>(T Input, Lazy<TestOutput> Output, object?[]? PresentedInput) : Test<T>
        {
            object? Test.Input => Input;
        }

        private record TestImpl(object? Input, Lazy<TestOutput> Output, object?[]? PresentedInput) : Test;

        public static Test<T> Create<T>(T input, Lazy<TestOutput> output, object?[]? presentedInput)
        {
            return new TestImpl<T>(input, output, presentedInput);
        }

        public static Test Create(object[] input, Lazy<TestOutput> output, object?[]? presentedInput)
        {
            return new TestImpl(input, output, presentedInput);
        }

        public static Test<T> Create<T>(T input, Func<bool> generateOutput, object?[]? presentedInput)
        {
            return new TestImpl<T>(input, AnalyzeBooleanOutput(generateOutput), presentedInput);
        }

        public static Test Create(object[] input, Func<bool> generateOutput, object?[]? presentedInput)
        {
            return new TestImpl(input, AnalyzeBooleanOutput(generateOutput), presentedInput);
        }

        private static Lazy<TestOutput> AnalyzeBooleanOutput(Func<bool> generateOutput) => new(() =>
        {
            try
            {
                return new TestOutputImpl(generateOutput() ? TestResult.Succeeded : TestResult.Failed, null);
            }
            catch (Property.PropertyPreconditionException)
            {
                return new TestOutputImpl(TestResult.FailedPrecondition, null);
            }
            catch (Exception ex)
            {
                return new TestOutputImpl(TestResult.Failed, ex);
            }
        });
        
        private record TestAsyncImpl<T>(T Input, Lazy<Task<TestOutput>> Output, object?[]? PresentedInput) : TestAsync<T>
        {
            object? TestAsync.Input => Input;
        }

        private record TestAsyncImpl(object? Input, Lazy<Task<TestOutput>> Output, object?[]? PresentedInput) : TestAsync;

        public static TestAsync<T> CreateAsync<T>(T input, Lazy<Task<TestOutput>> output, object?[]? presentedInput)
        {
            return new TestAsyncImpl<T>(input, output, presentedInput);
        }

        public static TestAsync CreateAsync(object[] input, Lazy<Task<TestOutput>> output, object?[]? presentedInput)
        {
            return new TestAsyncImpl(input, output, presentedInput);
        }

        public static TestAsync<T> CreateAsync<T>(T input, Func<Task<bool>> generateOutput, object?[]? presentedInput)
        {
            return new TestAsyncImpl<T>(input, AnalyzeBooleanOutput(generateOutput), presentedInput);
        }

        public static TestAsync CreateAsync(object[] input, Func<Task<bool>> generateOutput, object?[]? presentedInput)
        {
            return new TestAsyncImpl(input, AnalyzeBooleanOutput(generateOutput), presentedInput);
        }

        private static Lazy<Task<TestOutput>> AnalyzeBooleanOutput(Func<Task<bool>> generateOutput) => new(async () =>
        {
            try
            {
                return new TestOutputImpl(await generateOutput() ? TestResult.Succeeded : TestResult.Failed, null);
            }
            catch (Property.PropertyPreconditionException)
            {
                return new TestOutputImpl(TestResult.FailedPrecondition, null);
            }
            catch (Exception ex)
            {
                return new TestOutputImpl(TestResult.Failed, ex);
            }
        }, isThreadSafe: false);
    }
}
