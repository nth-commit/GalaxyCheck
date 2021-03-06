﻿using System;

namespace GalaxyCheck.Internal
{
    internal interface IEither<out TLeft, out TRight> { }

    internal abstract class Either<TLeft, TRight> : IEither<TLeft, TRight>
    {
        public static implicit operator Either<TLeft, TRight>(TLeft left)
        {
            return new Left<TLeft, TRight>(left);
        }

        public static implicit operator Either<TLeft, TRight>(TRight right)
        {
            return new Right<TLeft, TRight>(right);
        }
    }

    internal sealed class Left<TLeft, TRight> : Either<TLeft, TRight>
    {
        public Left(in TLeft left)
        {
            Value = left;
        }

        public TLeft Value { get; }

        public static implicit operator Left<TLeft, TRight>(TLeft left) => new Left<TLeft, TRight>(left);
        public static implicit operator TLeft(Left<TLeft, TRight> left) => left.Value;
    }

    internal class Right<TLeft, TRight> : Either<TLeft, TRight>
    {
        public Right(in TRight right)
        {
            Value = right;
        }

        public TRight Value { get; }

        public static implicit operator Right<TLeft, TRight>(TRight right) => new Right<TLeft, TRight>(right);
        public static implicit operator TRight(Right<TLeft, TRight> right) => right.Value;

    }

    internal static class EitherExtension
    {
        public static bool IsLeft<TLeft, TRight>(this Either<TLeft, TRight> either, out TLeft left)
        {
            if (either is Left<TLeft, TRight> l)
            {
                left = l.Value;
                return true;
            }

            left = default!;
            return false;
        }

        public static bool IsRight<TLeft, TRight>(this Either<TLeft, TRight> either, out TRight right)
        {
            if (either is Right<TLeft, TRight> r)
            {
                right = r.Value;
                return true;
            }

            right = default!;
            return false;
        }

        public static TResult Match<TLeft, TRight, TResult>(
            this Either<TLeft, TRight> either,
            Func<TLeft, TResult> fromLeft,
            Func<TRight, TResult> fromRight)
        {
            if (either.IsLeft(out TLeft left)) return fromLeft(left);
            if (either.IsRight(out TRight right)) return fromRight(right);
            throw new Exception("Fatal: Unhandled case");
        }
    }
}
