﻿using GalaxyCheck;
using Xunit;

namespace Tests
{
    public static class PropertyAssert
    {
        public static (CheckResult<T>, CheckResult<T>.CheckCounterexample) Falsifies<T>(
            IProperty<T> property,
            int seed,
            int? size = null,
            int iterations = 100)
        {
            var result = property.Check(iterations: iterations, seed: seed, size: size);

            Assert.True(result.Falsified);

            return (result, result.Counterexample!);
        }

        public static CheckResult<T> DoesNotFalsify<T>(
            IProperty<T> property,
            int seed,
            int? size = null,
            int iterations = 100)
        {
            var result = property.Check(iterations: iterations, seed: seed, size: size);

            Assert.False(result.Falsified);

            return result;
        }
    }
}
