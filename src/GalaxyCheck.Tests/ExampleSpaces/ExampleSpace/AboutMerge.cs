using FsCheck;
using FsCheck.Xunit;
using Newtonsoft.Json;
using Snapshooter;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ES = GalaxyCheck.ExampleSpaces;

namespace Tests.ExampleSpaces.ExampleSpace
{
    public class AboutMerge
    {
        private static readonly ES.ShrinkFunc<List<ES.IExampleSpace<int>>> NoShrink =
            ES.ShrinkFunc.None<List<ES.IExampleSpace<int>>>();

        private static readonly ES.MeasureFunc<List<ES.IExampleSpace<int>>> Unmeasured =
            ES.MeasureFunc.Unmeasured<List<ES.IExampleSpace<int>>>();

        [Theory]
        [InlineData(new [] { 1, 1 })]
        [InlineData(new[] { 2, 1 })]
        [InlineData(new[] { 1, 2 })]
        [InlineData(new[] { 1, 1, 1 })]
        [InlineData(new[] { 1, 2, 1 })]
        public void Snapshots_ArrayConcat_NoMergeShrink(int[] values)
        {
            var exampleSpaces = values
                .Select(value => ES.ExampleSpaceFactory.Unfold(
                    value,
                    ES.ShrinkFunc.Towards(0),
                    ES.MeasureFunc.Unmeasured<int>(),
                    ES.IdentifyFuncs.Default<int>()))
                .ToList();

            var mergedExampleSpace = ES.ExampleSpaceFactory.Merge(
                exampleSpaces,
                (values) => values.ToArray(),
                NoShrink,
                Unmeasured);

            Snapshot.Match(
                mergedExampleSpace.Render(values => JsonConvert.SerializeObject(values)),
                SnapshotNameExtension.Create(values.Select(x => x.ToString()).ToArray()));
        }

        [Theory]
        [InlineData(new[] { 1, 1 })]
        [InlineData(new[] { 2, 1 })]
        [InlineData(new[] { 1, 2 })]
        [InlineData(new[] { 1, 1, 1 })]
        [InlineData(new[] { 1, 2, 1 })]
        public void Snapshots_ArrayConcat_TowardsCountShrink(int[] values)
        {
            var exampleSpaces = values
                .Select(value => ES.ExampleSpaceFactory.Unfold(
                    value,
                    ES.ShrinkFunc.Towards(0),
                    ES.MeasureFunc.Unmeasured<int>(),
                    ES.IdentifyFuncs.Default<int>()))
                .ToList();

            var mergedExampleSpace = ES.ExampleSpaceFactory.Merge(
                exampleSpaces,
                (values) => values.ToArray(),
                ES.ShrinkFunc.TowardsCount<ES.IExampleSpace<int>, decimal>(0, es => es.Current.Distance),
                Unmeasured);

            Snapshot.Match(
                mergedExampleSpace.Render(values => JsonConvert.SerializeObject(values)),
                SnapshotNameExtension.Create(values.Select(x => x.ToString()).ToArray()));
        }

        [Property]
        public void ItMergesValuesUsingTheGivenFunction(List<int> values)
        {
            Func<IEnumerable<int>, int> mergeValues = xs => xs.Sum();
            var exampleSpaces = values.Select(ES.ExampleSpaceFactory.Singleton).ToList();

            var mergedExampleSpace = ES.ExampleSpaceFactory.Merge(exampleSpaces, mergeValues, NoShrink, Unmeasured);

            Assert.Equal(mergeValues(exampleSpaces.Select(x => x.Current.Value)), mergedExampleSpace.Current.Value);
        }

        [Property]
        public void ItMeasuresTheMergeUsingTheGivenFunction(
            List<int> values,
            Func<IEnumerable<int>, int> mergeValues,
            int mergeDistance)
        {
            ES.MeasureFunc<List<ES.IExampleSpace<int>>> measureMerge = _ => mergeDistance;
            var exampleSpaces = values.Select(ES.ExampleSpaceFactory.Singleton).ToList();

            var mergedExampleSpace = ES.ExampleSpaceFactory.Merge(exampleSpaces, mergeValues, NoShrink, measureMerge);

            Assert.Equal(mergeDistance, mergedExampleSpace.Current.Distance);
        }

        [Property]
        public FsCheck.Property ItAlwaysProducesADifferentId(
            object value,
            int mergeCount,
            Func<IEnumerable<object>, object> mergeValues)
        {
            Action test = () =>
            {
                var exampleSpace = ES.ExampleSpaceFactory.Singleton(value);

                var mergedExampleSpace = ES.ExampleSpaceFactory.Merge(
                    Enumerable.Repeat(exampleSpace, mergeCount).ToList(),
                    mergeValues,
                    ES.ShrinkFunc.None<List<ES.IExampleSpace<object>>>(),
                    ES.MeasureFunc.Unmeasured<List<ES.IExampleSpace<object>>>());

                Assert.NotEqual(exampleSpace.Current.Id, mergedExampleSpace.Current.Id);
            };

            return test.When(mergeCount > 0);
        }
    }
}
