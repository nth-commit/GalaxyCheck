#pragma warning disable IDE1006 // Naming Styles
using GalaxyCheck.Properties;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalaxyCheck
{
    public interface TestInput<out TInput, TOutput>
    {
        TInput Input { get; }

        Lazy<TOutput> Output { get; }

        IReadOnlyList<object?>? PresentedInput { get; }
    }

    public interface Test<out T> : TestInput<T, TestOutput>
    {
    }

    public interface Test : Test<object?>
    {
    }

    public interface AsyncTest<out T> : TestInput<T, Task<TestOutput>>
    {
    }

    public interface AsyncTest : AsyncTest<object?>
    {
    }

    public static partial class Extensions
    {
        public static Test<T> Cast<T>(this Test test) => TestFactory.Create((T)test.Input!, test.Output, test.PresentedInput);

        public static AsyncTest<T> Cast<T>(this AsyncTest test) => TestFactory.Create((T)test.Input!, test.Output, test.PresentedInput);

        public static AsyncTest<T> AsAsync<T>(this Test<T> test) => TestFactory.Create<T>(
            test.Input,
            new Lazy<Task<TestOutput>>(async () =>
            {
                await Task.CompletedTask;
                return test.Output.Value;
            }),
            test.PresentedInput);

    }
}
#pragma warning disable IDE1006 // Naming Styles
