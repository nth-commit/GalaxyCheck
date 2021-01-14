﻿using FsCheck.Xunit;
using Newtonsoft.Json;
using Snapshooter;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ES = GalaxyCheck.Internal.ExampleSpaces;

namespace Tests.ExampleSpaces.ExampleSpace
{
    public class AboutMerge
    {
        private static readonly ES.ShrinkFunc<IEnumerable<ES.ExampleSpace<int>>> NoShrink =
            ES.ShrinkFunc.None<IEnumerable<ES.ExampleSpace<int>>>();

        private static readonly ES.MeasureFunc<IEnumerable<ES.ExampleSpace<int>>> Unmeasured =
            ES.MeasureFunc.Unmeasured<IEnumerable<ES.ExampleSpace<int>>>();

        [Theory]
        [InlineData(new [] { 1, 1 })]
        [InlineData(new [] { 2, 1 })]
        [InlineData(new [] { 1, 2 })]
        [InlineData(new [] { 1, 1, 1 })]
        [InlineData(new [] { 1, 2, 1 })]
        public void Snapshots_ArrayConcat_NoMergeShrink(int[] values)
        {
            var exampleSpaces = values.Select(value => ES.ExampleSpace.Unfold(
                value,
                ES.ShrinkFunc.Towards(0),
                ES.MeasureFunc.Unmeasured<int>()));

            var mergedExampleSpace = ES.ExampleSpace.Merge(
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
            var exampleSpaces = values.Select(value => ES.ExampleSpace.Unfold(
                value,
                ES.ShrinkFunc.Towards(0),
                ES.MeasureFunc.Unmeasured<int>()));

            var mergedExampleSpace = ES.ExampleSpace.Merge(
                exampleSpaces,
                (values) => values.ToArray(),
                ES.ShrinkFunc.TowardsCount<ES.ExampleSpace<int>>(0),
                Unmeasured);

            Snapshot.Match(
                mergedExampleSpace.Render(values => JsonConvert.SerializeObject(values)),
                SnapshotNameExtension.Create(values.Select(x => x.ToString()).ToArray()));
        }

        [Property]
        public void ItMergesValuesUsingTheGivenFunction(List<int> values)
        {
            Func<IEnumerable<int>, int> mergeValues = xs => xs.Sum();
            var exampleSpaces = values.Select(ES.ExampleSpace.Singleton);

            var mergedExampleSpace = ES.ExampleSpace.Merge(exampleSpaces, mergeValues, NoShrink, Unmeasured);

            Assert.Equal(mergeValues(exampleSpaces.Select(x => x.Current.Value)), mergedExampleSpace.Current.Value);
        }

        [Property]
        public void ItMeasuresTheMergeUsingTheGivenFunction(List<int> values, Func<IEnumerable<int>, int> mergeValues, int mergeDistance)
        {
            ES.MeasureFunc<IEnumerable<ES.ExampleSpace<int>>> measureMerge = _ => mergeDistance;
            var exampleSpaces = values.Select(ES.ExampleSpace.Singleton);

            var mergedExampleSpace = ES.ExampleSpace.Merge(exampleSpaces, mergeValues, NoShrink, measureMerge);

            Assert.Equal(mergeDistance, mergedExampleSpace.Current.Distance);
        }
    }
}
