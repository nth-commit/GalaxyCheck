using GalaxyCheck.Internal.Gens;
using System;

namespace GalaxyCheck
{
    public delegate bool TestFunc<in T>(T value);

    public interface Test
    {
        TestFunc<object> Func { get; }

        object Input { get; }
    }

    public interface Test<T> : Test
    {
        new TestFunc<T> Func { get; }

        new T Input { get; }
    }

    public class PropertyOptions
    {
        public bool EnableLinqInference { get; init; }
    }

    public class Property : GenProvider<Test>, IGen<Test>
    {
        public record TestImpl(TestFunc<object> Func, object Input) : Test;

        private readonly IGen<Test> _gen;

        public Property(IGen<Test> gen, PropertyOptions? options = null)
        {
            _gen = gen;
            Options = options ?? new PropertyOptions { EnableLinqInference = false };
        }

        public PropertyOptions Options { get; }

        protected override IGen<Test> Gen => _gen;

        public static implicit operator Property<object>(Property p)
        {
            var gen =
                from i in p
                select new Property<object>.TestImpl(x => i.Func(x), i.Input);

            return new Property<object>(gen, p.Options);
        }

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

        public static Test ForThese(Action func) => ForThese(() =>
        {
            func();
            return true;
        });

        public static Test ForThese(Func<bool> func) => new TestImpl(
            (_) => func(),
            new Symbols.NoInput());
    }

    public class Property<T> : GenProvider<Test<T>>, IGen<Test<T>>
    {
        public record TestImpl(TestFunc<T> Func, T Input) : Test<T>
        {
            TestFunc<object> Test.Func => x => Func((T)x);

            object Test.Input => Input!;
        }

        private readonly IGen<Test<T>> _gen;

        public Property(IGen<Test<T>> gen, PropertyOptions? options = null)
        {
            _gen = gen;
            Options = options ?? new PropertyOptions { EnableLinqInference = false };
        }

        public PropertyOptions Options { get; }

        public Property(TestFunc<T> f, IGen<T> inputGen)
            : this(from x in inputGen
                   select new TestImpl(f, x))
        {
        }

        protected override IGen<Test<T>> Gen => _gen;

        public static implicit operator Property(Property<T> p)
        {
            var gen =
                from i in p
                select new Property.TestImpl(x => i.Func((T)x), i.Input);

            return new Property(gen, p.Options);
        }
    }
}