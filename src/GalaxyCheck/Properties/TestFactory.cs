using System;

namespace GalaxyCheck.Properties
{
    internal static class TestFactory
    {
        private record TestOutputImpl(Test.TestResult Result, Exception? Exception) : Test.TestOutput;

        private record TestImpl<T>(T Input, Lazy<Test.TestOutput> Output, object?[]? PresentedInput) : Test<T>
        {
            object? Test.Input => Input;
        }

        private record TestImpl(object? Input, Lazy<Test.TestOutput> Output, object?[]? PresentedInput) : Test;

        public static Test<T> Create<T>(T input, Lazy<Test.TestOutput> output, object?[]? presentedInput)
        {
            return new TestImpl<T>(input, output, presentedInput);
        }

        public static Test Create(object[] input, Lazy<Test.TestOutput> output, object?[]? presentedInput)
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

        private static Lazy<Test.TestOutput> AnalyzeBooleanOutput(Func<bool> generateOutput) => new Lazy<Test.TestOutput>(() =>
        {
            try
            {
                return new TestOutputImpl(generateOutput() ? Test.TestResult.Succeeded : Test.TestResult.Failed, null);
            }
            catch (Property.PropertyPreconditionException)
            {
                return new TestOutputImpl(Test.TestResult.FailedPrecondition, null);
            }
            catch (Exception ex)
            {
                return new TestOutputImpl(Test.TestResult.Failed, ex);
            }
        });
    }
}
