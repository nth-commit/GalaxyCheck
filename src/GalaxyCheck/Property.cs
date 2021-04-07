using System;
using System.Linq;

namespace GalaxyCheck
{
    public interface Test<T>
    {
        Func<T, bool> Func { get; }

        T Input { get; }

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
        public record TestImpl(Func<T, bool> Func, T Input, int Arity, bool EnableLinqInference, Func<T, object?[]> Present) : Test<T>
        {
        }

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
                select new GalaxyCheck.TestImpl(
                    _ => test.Func(test.Input),
                    new object[] { test.Input },
                    _ => test.Present(test.Input));

            return new Property(gen);
        }
    }

    internal record TestImpl<T>(Func<T, bool> Func, T Input, Func<T, object?[]> Present) : Test<T>
    {
        public int Arity => Present(Input).Length;
    }

    internal record TestImpl(Func<object[], bool> Func, object[] Input, Func<object[], object?[]> Present) : Test
    {
        public int Arity => Present(Input).Length;
    }

}