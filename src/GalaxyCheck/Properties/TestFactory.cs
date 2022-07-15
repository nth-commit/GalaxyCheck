﻿using System;
using System.Collections.Generic;

namespace GalaxyCheck.Properties
{
    internal static class TestFactory
    {
        private record TestOutputImpl(TestResult Result, Exception? Exception) : TestOutput;

        private record TestImpl<T>(T Input, Lazy<TestOutput> Output, IReadOnlyList<object?>? PresentedInput) : Test<T>;

        private record TestImpl(object? Input, Lazy<TestOutput> Output, IReadOnlyList<object?>? PresentedInput) : Test;

        public static Test<T> Create<T>(T input, Lazy<TestOutput> output, IReadOnlyList<object?>? presentedInput)
        {
            return new TestImpl<T>(input, output, presentedInput);
        }

        public static Test Create(object[] input, Lazy<TestOutput> output, IReadOnlyList<object?>? presentedInput)
        {
            return new TestImpl(input, output, presentedInput);
        }

        public static Test<T> Create<T>(T input, Func<bool> generateOutput, IReadOnlyList<object?>? presentedInput)
        {
            return new TestImpl<T>(input, AnalyzeBooleanOutput(generateOutput), presentedInput);
        }

        public static Test Create(object[] input, Func<bool> generateOutput, IReadOnlyList<object?>? presentedInput)
        {
            return new TestImpl(input, AnalyzeBooleanOutput(generateOutput), presentedInput);
        }

        private static Lazy<TestOutput> AnalyzeBooleanOutput(Func<bool> generateOutput) => new Lazy<TestOutput>(() =>
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
    }
}
