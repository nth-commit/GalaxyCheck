using System;

namespace GalaxyCheck
{
    public enum TestResult
    {
        Succeeded = 1,
        Failed = 2,
        FailedPrecondition = 3
    }

    public interface TestOutput
    {
        TestResult Result { get; }

        Exception? Exception { get; }
    }

    public interface Test
    {
        object? Input { get; }

        Lazy<TestOutput> Output { get; }

        object?[]? PresentedInput { get; }
    }

    public interface Test<out T> : Test
    {
        new T Input { get; }
    }

    internal static class TestFactory
    {
        private record TestOutputImpl(TestResult Result, Exception? Exception) : TestOutput;

        private record TestImpl<T>(T Input, Lazy<TestOutput> Output, object?[]? PresentedInput) : Test<T>
        {
            object? Test.Input => Input;
        }

        private record TestImpl(object? Input, Lazy<TestOutput> Output, object?[]? PresentedInput) : Test;

        public static Test<T> Create<T>(T Input, Lazy<bool> Output, object?[]? PresentedInput)
        {
            return new TestImpl<T>(Input, AnalyzeBooleanOutput(Output), PresentedInput);
        }

        public static Test<T> Create<T>(T Input, Lazy<TestOutput> Output, object?[]? PresentedInput)
        {
            return new TestImpl<T>(Input, Output, PresentedInput);
        }

        public static Test Create(object[] Input, Lazy<bool> Output, object?[]? PresentedInput)
        {
            return new TestImpl(Input, AnalyzeBooleanOutput(Output), PresentedInput);
        }

        public static Test Create(object[] Input, Lazy<TestOutput> Output, object?[]? PresentedInput)
        {
            return new TestImpl(Input, Output, PresentedInput);
        }

        private static Lazy<TestOutput> AnalyzeBooleanOutput(Lazy<bool> output) => new Lazy<TestOutput>(() =>
        {
            try
            {
                return new TestOutputImpl(output.Value ? TestResult.Succeeded : TestResult.Failed, null);
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

    public static class TestExtensions
    {
        public static Test<T> Cast<T>(this Test test) => TestFactory.Create((T)test.Input!, test.Output, test.PresentedInput);
    }

    public partial class Property : IGen<Test>
    {
        private readonly IGen<Test> _gen;

        public Property(IGen<Test> gen)
        {
            _gen = gen;
        }

        public IGenAdvanced<Test> Advanced => _gen.Advanced;

        IGenAdvanced IGen.Advanced => Advanced;

        public class PropertyPreconditionException : Exception
        {
        }

        public static void Precondition(bool condition)
        {
            if (!condition)
            {
                throw new PropertyPreconditionException();
            }
        }
    }

    public class Property<T> : IGen<Test<T>>
    {
        private readonly IGen<Test<T>> _gen;

        public Property(IGen<Test<T>> gen)
        {
            _gen = gen;
        }

        public IGenAdvanced<Test<T>> Advanced => _gen.Advanced;

        IGenAdvanced IGen.Advanced => Advanced;

        public static implicit operator Property(Property<T> property) => new Property(
            from test in property
            select TestFactory.Create(new object[] { test.Input }, test.Output, test.PresentedInput));
    }
}