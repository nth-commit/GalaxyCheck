using System;

namespace GalaxyCheck
{
    public interface Test<out T>
    {
        T Input { get; }

        Lazy<bool> Output { get; }

        object?[]? PresentedInput { get; }
    }

    public interface Test : Test<object[]>
    {
    }

    internal static class TestFactory
    {
        private record TestImpl<T>(T Input, Lazy<bool> Output, object?[]? PresentedInput) : Test<T>;

        private record TestImpl(object[] Input, Lazy<bool> Output, object?[]? PresentedInput) : Test;

        public static Test<T> Create<T>(T Input, Lazy<bool> Output, object?[]? PresentedInput)
        {
            return new TestImpl<T>(Input, Output, PresentedInput);
        }

        public static Test Create(object[] Input, Lazy<bool> Output, object?[]? PresentedInput)
        {
            return new TestImpl(Input, Output, PresentedInput);
        }
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