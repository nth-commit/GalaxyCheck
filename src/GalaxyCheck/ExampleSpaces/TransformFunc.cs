using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
{
    public delegate IEnumerable<TResult> TransformFunc<TSource, TResult>(TSource value);

    public static class TransformShrinksFunc
    {
        public static TransformFunc<T, T> Identity<T>() => (T value) => new[] { value };

        public static TransformFunc<TSource, TResult> ThenWhere<TSource, TResult>(
            this TransformFunc<TSource, TResult> f,
            Func<TResult, bool> pred) =>
                (TSource value) => f(value).Where(pred);

        public static TransformFunc<TSource, TProjection> ThenSelect<TSource, TResult, TProjection>(
            this TransformFunc<TSource, TResult> f,
            Func<TResult, TProjection> selector) =>
                (TSource value) => f(value).Select(selector);
    }
}
