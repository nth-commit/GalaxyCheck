using GalaxyCheck.Properties;
using System;
using System.Threading.Tasks;

namespace GalaxyCheck
{
#pragma warning disable IDE1006 // Naming Styles
    public interface TestAsync
#pragma warning restore IDE1006 // Naming Styles
    {
        object? Input { get; }

        Lazy<Task<TestOutput>> Output { get; }

        object?[]? PresentedInput { get; }

    }
    
#pragma warning disable IDE1006 // Naming Styles
    public interface TestAsync<out T> : TestAsync
#pragma warning restore IDE1006 // Naming Styles
    {
        new T Input { get; }
    }

    public static partial class Extensions
    {
        public static TestAsync<T> Cast<T>(this TestAsync test) => TestFactory.CreateAsync((T)test.Input!, test.Output, test.PresentedInput);
    }
}
