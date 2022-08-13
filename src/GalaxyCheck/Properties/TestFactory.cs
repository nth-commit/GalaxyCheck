using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalaxyCheck.Properties
{
    internal static class TestFactory
    {
        private record TestOutputImpl(Property.TestResult Result, Exception? Exception) : Property.TestOutput;

        private record TestImpl<T>(T Input, Lazy<Property.TestOutput> Output, IReadOnlyList<object?> PresentedInput) : Property.Test<T>;

        private record TestImpl(object? Input, Lazy<Property.TestOutput> Output, IReadOnlyList<object?> PresentedInput) : Property.Test;

        public static Property.Test<T> Create<T>(T input, Lazy<Property.TestOutput> output, IReadOnlyList<object?> presentedInput)
        {
            return new TestImpl<T>(input, output, presentedInput);
        }

        public static Property.Test<T> Create<T>(T input, Func<bool> generateOutput, IReadOnlyList<object?> presentedInput)
        {
            return new TestImpl<T>(input, AnalyzeBooleanOutput(generateOutput), presentedInput);
        }

        public static Property.Test Create(object[] input, Func<bool> generateOutput, IReadOnlyList<object?> presentedInput)
        {
            return new TestImpl(input, AnalyzeBooleanOutput(generateOutput), presentedInput);
        }

        private static Lazy<Property.TestOutput> AnalyzeBooleanOutput(Func<bool> generateOutput) => new Lazy<Property.TestOutput>(() =>
        {
            try
            {
                return new TestOutputImpl(generateOutput() ? Property.TestResult.Succeeded : Property.TestResult.Failed, null);
            }
            catch (Exception ex)
            {
                return new TestOutputImpl(Property.TestResult.Failed, ex);
            }
        });

        private record AsyncTestImpl<T>(T Input, Lazy<ValueTask<Property.TestOutput>> Output, IReadOnlyList<object?> PresentedInput) : Property.AsyncTest<T>;

        private record AsyncTestImpl(object? Input, Lazy<ValueTask<Property.TestOutput>> Output, IReadOnlyList<object?> PresentedInput) : Property.AsyncTest;

        public static Property.AsyncTest<T> Create<T>(T input, Lazy<ValueTask<Property.TestOutput>> output, IReadOnlyList<object?> presentedInput)
        {
            return new AsyncTestImpl<T>(input, output, presentedInput);
        }

        public static Property.AsyncTest<T> Create<T>(T input, Func<ValueTask<bool>> generateOutput, IReadOnlyList<object?> presentedInput)
        {
            return new AsyncTestImpl<T>(input, AnalyzeBooleanOutput(generateOutput), presentedInput);
        }

        public static Property.AsyncTest Create(object[] input, Func<ValueTask<bool>> generateOutput, IReadOnlyList<object?> presentedInput)
        {
            return new AsyncTestImpl(input, AnalyzeBooleanOutput(generateOutput), presentedInput);
        }

        private static Lazy<ValueTask<Property.TestOutput>> AnalyzeBooleanOutput(Func<ValueTask<bool>> generateOutput) => new Lazy<ValueTask<Property.TestOutput>>(async () =>
        {
            try
            {
                return new TestOutputImpl(await generateOutput() ? Property.TestResult.Succeeded : Property.TestResult.Failed, null);
            }
            catch (Exception ex)
            {
                return new TestOutputImpl(Property.TestResult.Failed, ex);
            }
        }, isThreadSafe: false);
    }
}
