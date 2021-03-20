using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Gens.Internal
{
    internal record WeightedElement<T>(int Weight, T Element);

    internal class WeightedList<T> : IReadOnlyList<T>
    {
        private readonly int _totalWeight;
        private readonly ImmutableList<ElementAndIndexRange> _valueAndIndexRanges;

        public WeightedList(IEnumerable<WeightedElement<T>> weightedElements)
        {
            if (weightedElements.Any(s => s.Weight < 0))
                throw new ArgumentException("Weighted element must be positive", nameof(weightedElements));

            _totalWeight = weightedElements.Select(x => x.Weight).Sum();

            _valueAndIndexRanges = weightedElements
                .Where(s => s.Weight > 0)
                .Aggregate(
                    ImmutableList.Create<ElementAndIndexRange>(),
                    (acc, curr) =>
                    {
                        var lastMaxIndex = acc.LastOrDefault()?.MaxIndex ?? -1;
                        var minIndex = lastMaxIndex + 1;
                        var maxIndex = minIndex + curr.Weight - 1;
                        return acc.Add(new ElementAndIndexRange(curr.Element, minIndex, maxIndex));
                    });
        }

        public T this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative");

                if (index >= Count)
                    throw new ArgumentOutOfRangeException(
                        nameof(index),
                        "Index must be less than the total weight of the elements");

                return _valueAndIndexRanges
                    .Where(x => x.MinIndex <= index && index <= x.MaxIndex)
                    .Select(x => x.Element)
                    .Single();
            }
        }

        public int Count => _totalWeight;

        public IEnumerator<T> GetEnumerator() => Enumerable
            .Range(0, Count)
            .Select(index => this[index])
            .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private record ElementAndIndexRange(T Element, int MinIndex, int MaxIndex);
    }
}
