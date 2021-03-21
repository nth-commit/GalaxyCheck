using FluentAssertions;
using GalaxyCheck.Gens.Parameters;
using NebulaCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System.Linq;
using Xunit;
using static GalaxyCheck.Gens.Parameters.Internal.SamplingSize;

namespace Tests.V2.ImplementationTests.SizingTests
{
    public class AboutSamplingSize
    {
        [Property]
        public IGen<Test> IfTheNumberOfSamplesIsWithinTheSizeRange_TheSampleHasTheNumberOfSamples() =>
            from numberOfSamples in Gen.Int32().Between(0, 100)
            select Property.ForThese(() =>
            {
                var sample = SampleSize(numberOfSamples);

                sample.Should().HaveCount(numberOfSamples);
            });

        [Property]
        public IGen<Test> IfTheNumberOfSamplesIsGreaterThanZero_TheLastSizeIsOneHundred() =>
            from numberOfSamples in Gen.Int32().GreaterThan(0)
            select Property.ForThese(() =>
            {
                var sample = SampleSize(numberOfSamples);

                sample.Should().EndWith(new Size(100));
            });

        [Property]
        public IGen<Test> IfTheNumberOfSamplesIsGreaterThanOne_TheFirstSizeIsZero() =>
            from numberOfSamples in Gen.Int32().GreaterThan(1)
            select Property.ForThese(() =>
            {
                var sample = SampleSize(numberOfSamples);

                sample.Should().StartWith(new Size(0));
            });

        [Property]
        public IGen<Test> ThereAreOnlyUniqueSizesWithinTheSample() =>
            from numberOfSamples in Gen.Int32().GreaterThanEqual(0)
            select Property.ForThese(() =>
            {
                var sample = SampleSize(numberOfSamples);

                sample.Should().OnlyHaveUniqueItems();
            });

        [Theory]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(33)]
        public void Snapshots(int numberOfSamples)
        {
            var sample = SampleSize(numberOfSamples);

            Snapshot.Match(sample, SnapshotNameExtension.Create($"NumberOfSamples={numberOfSamples}"));
        }
    }
}
