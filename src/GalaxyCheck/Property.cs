using System;

namespace GalaxyCheck
{
    public interface Test<T>
    {
        T Input { get; }

        Lazy<bool> Output { get; }

        int Arity { get; }

        Func<T, object?[]> Present { get; }
    }

    public interface Test : Test<object[]>
    {
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

        public static implicit operator Property(Property<T> property)
        {
            var gen =
                from test in property
                select new TestImpl(
                    new object[] { test.Input },
                    test.Output,
                    _ => test.Present(test.Input));

            return new Property(gen);
        }
    }

    internal record TestImpl<T>(T Input, Lazy<bool> Output, Func<T, object?[]> Present) : Test<T>
    {
        public int Arity => Present(Input).Length;
    }

    internal record TestImpl(object[] Input, Lazy<bool> Output, Func<object[], object?[]> Present) : Test
    {
        public int Arity => Present(Input).Length;
    }
}