using GalaxyCheck.Properties;
using System;
using System.Collections.Generic;

namespace GalaxyCheck
{
#pragma warning disable IDE1006 // Naming Styles
    public interface Test : Test<object?>
#pragma warning restore IDE1006 // Naming Styles
    {
    }

#pragma warning disable IDE1006 // Naming Styles
    public interface Test<out T>
#pragma warning restore IDE1006 // Naming Styles
    {
        T Input { get; }

        Lazy<TestOutput> Output { get; }

        IReadOnlyList<object?>? PresentedInput { get; }
    }

    public static partial class Extensions
    {
        public static Test<T> Cast<T>(this Test test) => TestFactory.Create((T)test.Input!, test.Output, test.PresentedInput);
    }
}
