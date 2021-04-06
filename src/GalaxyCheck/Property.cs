﻿using System;
using System.Linq;

namespace GalaxyCheck
{
    public interface Test<T>
    {
        Func<T, bool> Func { get; }

        T Input { get; }

        int Arity { get; }

        bool EnableLinqInference { get; }
    }

    public interface Test : Test<object[]>
    {
    }

    public partial class Property : IGen<Test>
    {
        public record TestImpl(Func<object[], bool> Func, object[] Input, int Arity, bool EnableLinqInference) : Test;

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
        public record TestImpl(Func<T, bool> Func, T Input, int Arity, bool EnableLinqInference) : Test<T>
        {
        }

        private readonly IGen<Test<T>> _gen;

        public Property(IGen<Test<T>> gen)
        {
            _gen = gen;
        }

        public Property(IGen<T> inputGen, Func<T, bool> f, int arity)
            : this(
                from x in inputGen
                select new TestImpl(f, x, arity, x is Test test && test.EnableLinqInference)) // I don't like this code.
        {
        }

        public IGenAdvanced<Test<T>> Advanced => _gen.Advanced;

        IGenAdvanced IGen.Advanced => Advanced;

        public static implicit operator Property(Property<T> p)
        {
            var gen =
                from i in p
                select new Property.TestImpl(x => i.Func((T)x.Single()), new object[] { i.Input }, i.Arity, i.EnableLinqInference);

            return new Property(gen);
        }
    }
}