using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using GC = GalaxyCheck;
using GalaxyCheck.Internal.ExampleSpaces;

namespace Tests.ExampleSpaces.ExampleSpace
{
    public class AboutCounterexamples
    {
        [Property]
        public void WhenPredicateIsAlwaysTrue_ItReturnsAnEmptyEnumerable(
            object value,
            Func<object, List<object>> shrink,
            Func<object, decimal> measure)
        {
            var exampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Unfold(value, shrink.Invoke, measure.Invoke);

            var counterexamples = exampleSpace.Counterexamples(_ => true);

            Assert.Empty(counterexamples);
        }

        [Fact]
        public void WhenPredicateIsAlwaysFalseAndExampleSpaceIsSingleton_ItReturnsASingleCounterexample()
        {
            var exampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Singleton(1);

            var counterexamples = exampleSpace.Counterexamples(_ => false);

            Assert.Single(counterexamples);
        }

        [Fact]
        public void WhenPredicateAlwaysThrowsAndExampleSpaceIsSingleton_ItReturnsASingleCounterexample()
        {
            var exception = new Exception();
            var exampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Singleton(1);

            var counterexamples = exampleSpace.Counterexamples(_ =>
            {
                throw exception;
            });

            var counterexample = Assert.Single(counterexamples);
            Assert.Equal(exception, counterexample.Exception);
        }

        [Property]
        public FsCheck.Property WhenPredicateIsAlwaysFalseAndSubspaceIsNotEmpty_ItReturnsAnEnumerableWithLengthGreaterThanOne(
            object value,
            Func<object, List<object>> shrink,
            Func<object, decimal> measure)
        {
            var exampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Unfold(value, shrink.Invoke, measure.Invoke);

            Action test = () =>
            {
                var counterexamples = exampleSpace.Counterexamples(_ => false).Take(10);

                Assert.True(counterexamples.Count() > 1);
            };

            return test.When(exampleSpace.Subspace.Any());
        }

        [Property]
        public void CounterexamplesCanBeRelocated(
            object value,
            Func<object, List<object>> shrink,
            Func<object, decimal> measure,
            Func<object, bool> pred)
        {
            var exampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Unfold(value, shrink.Invoke, measure.Invoke);

            var counterexamples = exampleSpace.Counterexamples(pred).Take(10);

            Assert.All(counterexamples, counterexample =>
            {
                Assert.Equal(counterexample.Value, exampleSpace.Navigate(counterexample.Path.ToList())?.Value);
            });
        }

        [Property]
        public void CounterexamplesHaveAnIncrementingPathLength(
            object value,
            Func<object, List<object>> shrink,
            Func<object, decimal> measure,
            Func<object, bool> pred)
        {
            var exampleSpace = GC.Internal.ExampleSpaces.ExampleSpace.Unfold(value, shrink.Invoke, measure.Invoke);

            var counterexamples = exampleSpace.Counterexamples(pred).Take(10);

            Assert.All(
                counterexamples.Select((counterexample, index) => new { counterexample, index }),
                (x) =>
                {
                    Assert.Equal(x.index + 1, x.counterexample.Path.Count());
                });
        }
    }
}
