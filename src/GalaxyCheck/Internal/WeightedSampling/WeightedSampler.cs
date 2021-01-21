using System;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Internal.WeightedSampling
{
    public class WeightedSampler<T>
    {
        private readonly int _totalWeight;
        private readonly ImmutableList<ValueAndIndexRange> _valueAndIndexRanges;

        public WeightedSampler(ImmutableList<WeightedSample<T>> weightedSamples)
        {
            if (weightedSamples.Any() == false)
                throw new ArgumentException("weighted samples cannot be empty", nameof(weightedSamples));

            if (weightedSamples.Any(s => s.Weight < 0))
                throw new ArgumentException("weighted sample must be positive", nameof(weightedSamples));

            var totalWeight = weightedSamples.Select(x => x.Weight).Sum();
            if (totalWeight == 0)
                throw new ArgumentException("total weight must be greater than zero", nameof(weightedSamples));

            _totalWeight = totalWeight;

            _valueAndIndexRanges = weightedSamples
                .Where(s => s.Weight > 0)
                .Aggregate(
                    ImmutableList.Create<ValueAndIndexRange>(),
                    (acc, curr) =>
                    {
                        var lastMaxIndex = acc.LastOrDefault()?.MaxIndex ?? -1;
                        var minIndex = lastMaxIndex + 1;
                        var maxIndex = minIndex + curr.Weight - 1;
                        return acc.Add(new ValueAndIndexRange(curr.Value, minIndex, maxIndex));
                    });
        }

        public T? Sample(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "index must be non-negative");

            if (index > MaxIndex)
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    "index must be less than the total weight of the samples");

            return _valueAndIndexRanges
                .Where(x => x.MinIndex <= index && index <= x.MaxIndex)
                .Select(x => x.Value)
                .Single();
        }

        public int MaxIndex => _totalWeight - 1;

        private record ValueAndIndexRange(T Value, int MinIndex, int MaxIndex);
    }

    public record WeightedSample<T>(int Weight, T Value);
}
