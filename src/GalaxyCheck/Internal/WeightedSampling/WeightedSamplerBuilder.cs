using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GalaxyCheck.Internal.WeightedSampling
{
    public class WeightedSamplerBuilder<T>
    {
        public static WeightedSamplerBuilder<TBuilder> Create<TBuilder>() =>
            new WeightedSamplerBuilder<TBuilder>(ImmutableList.Create<WeightedSample<TBuilder>>());

        private readonly ImmutableList<WeightedSample<T>> _samples;

        public WeightedSamplerBuilder()
            : this(ImmutableList.Create<WeightedSample<T>>()) { }

        private WeightedSamplerBuilder(ImmutableList<WeightedSample<T>> samples)
        {
            _samples = samples;
        }

        public WeightedSamplerBuilder<T> WithSample(WeightedSample<T> sample) =>
            new WeightedSamplerBuilder<T>(_samples.Add(sample));

        public WeightedSamplerBuilder<T> WithSample(int weight, T value) =>
            WithSample(new WeightedSample<T>(weight, value));

        public WeightedSamplerBuilder<T> WithSamples(params WeightedSample<T>[] samples) =>
            samples.Aggregate(this, (acc, curr) => acc.WithSample(curr));

        public WeightedSampler<T> Build() => new WeightedSampler<T>(_samples);
    }
}
