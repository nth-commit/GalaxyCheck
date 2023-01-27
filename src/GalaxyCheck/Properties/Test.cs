#pragma warning disable IDE1006 // Naming Styles
using GalaxyCheck.Properties;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalaxyCheck
{
    public partial class Property
    {
        public interface TestInput<out TInput>
        {
            TInput Input { get; }

            IReadOnlyList<object?> PresentedInput { get; }
        }

        public interface Test<out T> : TestInput<T>
        {
            Lazy<TestOutput> Output { get; }
        }

        public interface Test : Test<object?>
        {
        }

        public interface AsyncTest<out T> : TestInput<T>
        {
            Lazy<ValueTask<TestOutput>> Output { get; }
        }

        public interface AsyncTest : AsyncTest<object?>
        {
        }
    }

    public static partial class Extensions
    {
        public static Property.Test<T> Cast<T>(this Property.Test test) =>
            TestFactory.Create((T)test.Input!, test.Output, test.PresentedInput);

        public static Property.AsyncTest<T> Cast<T>(this Property.AsyncTest test) =>
            TestFactory.Create((T)test.Input!, test.Output, test.PresentedInput);

        public static Property.AsyncTest<T> AsAsync<T>(this Property.Test<T> test) => TestFactory.Create<T>(
            test.Input,
            new Lazy<ValueTask<Property.TestOutput>>(() => ValueTask.FromResult(test.Output.Value)),
            test.PresentedInput);
    }
}

#pragma warning disable IDE1006 // Naming Styles
