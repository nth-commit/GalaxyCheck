using GalaxyCheck.Internal.Gens;
using System;

namespace GalaxyCheck
{
    public delegate bool PropertyFunc<in T>(T value);

    public interface IPropertyIteration
    {
        PropertyFunc<object> Func { get; }

        object Input { get; }
    }

    public interface IPropertyIteration<T> : IPropertyIteration
    {
        new PropertyFunc<T> Func { get; }

        new T Input { get; }
    }

    public class Property : GenProvider<IPropertyIteration>, IGen<IPropertyIteration>
    {
        public record Iteration(PropertyFunc<object> Func, object Input) : IPropertyIteration;

        private readonly IGen<IPropertyIteration> gen;

        public Property(IGen<IPropertyIteration> gen)
        {
            this.gen = gen;
        }

        protected override IGen<IPropertyIteration> Gen => gen;

        public static implicit operator Property<object>(Property p)
        {
            var gen =
                from i in p
                select new Property<object>.Iteration(x => i.Func(x), i.Input);

            return new Property<object>(gen);
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
    }

    public class Property<T> : GenProvider<IPropertyIteration<T>>, IGen<IPropertyIteration<T>>
    {
        public record Iteration(PropertyFunc<T> Func, T Input) : IPropertyIteration<T>
        {
            PropertyFunc<object> IPropertyIteration.Func => x => Func((T)x);

            object IPropertyIteration.Input => Input!;
        }

        private readonly IGen<IPropertyIteration<T>> gen;

        public Property(IGen<IPropertyIteration<T>> gen)
        {
            this.gen = gen;
        }

        public Property(PropertyFunc<T> f, IGen<T> inputGen)
            : this(from x in inputGen
                   select new Iteration(f, x))
        {
        }

        protected override IGen<IPropertyIteration<T>> Gen => gen;

        public static implicit operator Property(Property<T> p)
        {
            var gen =
                from i in p
                select new Property.Iteration(x => i.Func((T)x), i.Input);

            return new Property(gen);
        }
    }
}