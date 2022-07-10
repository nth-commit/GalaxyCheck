using GalaxyCheck.Properties;
using System;

namespace GalaxyCheck
{
#pragma warning disable IDE1006 // Naming Styles
    public interface Test
#pragma warning restore IDE1006 // Naming Styles
    {
        object? Input { get; }

        Lazy<TestOutput> Output { get; }

        object?[]? PresentedInput { get; }

#pragma warning disable IDE1006 // Naming Styles
        public interface TestOutput
#pragma warning restore IDE1006 // Naming Styles
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

#pragma warning disable IDE1006 // Naming Styles
    public interface Test<out T> : Test
#pragma warning restore IDE1006 // Naming Styles
    {
        new T Input { get; }
    }

    public static partial class Extensions
    {
        public static Test<T> Cast<T>(this Test test) => TestFactory.Create((T)test.Input!, test.Output, test.PresentedInput);
    }
}
