using GalaxyCheck.Properties;
using System;

namespace GalaxyCheck
{
    public interface Test
    {
        object? Input { get; }

        Lazy<TestOutput> Output { get; }

        object?[]? PresentedInput { get; }

        public interface TestOutput
        {
            TestResult Result { get; }

            Exception? Exception { get; }
        }

        public enum TestResult
        {
            Succeeded = 1,
            Failed = 2,
            FailedPrecondition = 3
        }
    }

    public interface Test<out T> : Test
    {
        new T Input { get; }
    }

    public static partial class Extensions
    {
        public static Test<T> Cast<T>(this Test test) => TestFactory.Create((T)test.Input!, test.Output, test.PresentedInput);
    }
}
