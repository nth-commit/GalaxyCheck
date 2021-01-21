using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;
using WS = GalaxyCheck.Internal.WeightedSampling;

namespace Tests.WeightedSampler
{
    [Properties(Arbitrary = new[] { typeof(Arbitrary) })]
    public class AboutWeightedSampler
    {
        public static class Arbitrary
        {
            public static Arbitrary<WS.WeightedSample<object>> WeightedSample() => (
                from weight in FsCheck.Gen.Choose(0, 100)
                from value in FsCheck.Arb.Default.String().Generator
                select new WS.WeightedSample<object>(weight, value)).ToArbitrary();
        }

        [Fact]
        public void ItThrowsWhenListIsEmpty()
        {
            Action throwing = () => new WS.WeightedSamplerBuilder<object>().Build();

            var exception = Assert.Throws<ArgumentException>("weightedSamples", throwing);
            Assert.Equal("weighted samples cannot be empty (Parameter 'weightedSamples')", exception.Message);
        }

        [Property]
        public FsCheck.Property ItThrowsWhenAWeightIsNegative(int weight, object value)
        {
            Action test = () =>
            {
                Action throwing = () => new WS.WeightedSamplerBuilder<object>()
                    .WithSample(weight, value)
                    .Build();

                var exception = Assert.Throws<ArgumentException>("weightedSamples", throwing);
                Assert.Equal("weighted sample must be positive (Parameter 'weightedSamples')", exception.Message);
            };

            return test.When(weight < 0);
        }

        [Property]
        public FsCheck.Property ItThrowsWhenTotalWeightIsZero(List<object> values)
        {
            Action test = () =>
            {
                Action throwing = () => new WS.WeightedSamplerBuilder<object>()
                    .WithSamples(values
                        .Select(x => new WS.WeightedSample<object>(0, x))
                        .ToArray())
                    .Build();

                var exception = Assert.Throws<ArgumentException>("weightedSamples", throwing);
                Assert.Equal("total weight must be greater than zero (Parameter 'weightedSamples')", exception.Message);
            };

            return test.When(values.Any());
        }

        [Property]
        public FsCheck.Property ItThrowsWhenTheSampleIndexIsNegative(List<WS.WeightedSample<object>> samples, int index)
        {
            Action test = () =>
            {
                var sampler = new WS.WeightedSamplerBuilder<object>()
                    .WithSamples(samples.ToArray())
                    .Build();

                Action throwing = () =>
                {
                    sampler.Sample(index);
                };

                var exception = Assert.Throws<ArgumentOutOfRangeException>("index", throwing);
                Assert.Equal("index must be non-negative (Parameter 'index')", exception.Message);
            };

            return test.When(samples.Any(s => s.Weight > 0) && index < 0);
        }

        [Property]
        public FsCheck.Property ItThrowsWhenTheSampleIndexIsGreaterThanTheWeights(List<WS.WeightedSample<object>> samples, int indexOffset)
        {
            Action test = () =>
            {
                var sampler = new WS.WeightedSamplerBuilder<object>()
                    .WithSamples(samples.ToArray())
                    .Build();

                var index = samples.Select(x => x.Weight).Sum() + indexOffset;

                Action throwing = () =>
                {
                    sampler.Sample(index);
                };

                var exception = Assert.Throws<ArgumentOutOfRangeException>("index", throwing);
                Assert.Equal("index must be less than the total weight of the samples (Parameter 'index')", exception.Message);
            };

            return test.When(samples.Any(s => s.Weight > 0) && indexOffset > 0);
        }

        [Property]
        public FsCheck.Property ItReturnsTheFirstSampleWhenGivenIndexZero(
            WS.WeightedSample<object> first,
            List<WS.WeightedSample<object>> rest)
        {
            Action test = () =>
            {
                var sampler = new WS.WeightedSamplerBuilder<object>()
                    .WithSample(first)
                    .WithSamples(rest.ToArray())
                    .Build();

                var value = sampler.Sample(0);

                Assert.Equal(first.Value, value);
            };

            return test.When(first.Weight > 0);
        }

        [Property]
        public FsCheck.Property ItReturnsTheFirstSampleWhenGivenIndexIsTheFirstWeightLessOne(
            WS.WeightedSample<object> first,
            List<WS.WeightedSample<object>> rest)
        {
            Action test = () =>
            {
                var sampler = new WS.WeightedSamplerBuilder<object>()
                    .WithSample(first)
                    .WithSamples(rest.ToArray())
                    .Build();

                var value = sampler.Sample(first.Weight - 1);

                Assert.Equal(first.Value, value);
            };

            return test.When(first.Weight > 0);
        }

        [Property]
        public FsCheck.Property ItReturnsTheSecondSampleWhenGivenIndexIsTheFirstWeight(
            WS.WeightedSample<object> first,
            WS.WeightedSample<object> second,
            List<WS.WeightedSample<object>> rest)
        {
            Action test = () =>
            {
                var sampler = new WS.WeightedSamplerBuilder<object>()
                    .WithSample(first)
                    .WithSample(second)
                    .WithSamples(rest.ToArray())
                    .Build();

                var value = sampler.Sample(first.Weight);

                Assert.Equal(second.Value, value);
            };

            return test.When(second.Weight > 0);
        }

        [Property]
        public FsCheck.Property ItReturnsTheLastSampleWhenGivenIndexIsTheMaxIndex(
            List<WS.WeightedSample<object>> rest,
            WS.WeightedSample<object> last)
        {
            Action test = () =>
            {
                var sampler = new WS.WeightedSamplerBuilder<object>()
                    .WithSamples(rest.ToArray())
                    .WithSample(last)
                    .Build();

                var value = sampler.Sample(sampler.MaxIndex);

                Assert.Equal(last.Value, value);
            };

            return test.When(last.Weight > 0);
        }
    }
}
