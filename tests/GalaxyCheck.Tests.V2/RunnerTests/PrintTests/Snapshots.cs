using GalaxyCheck;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.V2.RunnerTests.PrintTests
{
    public class Snapshots
    {
        [Fact]
        public void PrintInt32Gen()
        {
            var gen = Gen.Int32();

            var print = PrintAndCollect(gen.Cast<object>());

            Snapshot.Match(print);
        }

        [Fact]
        public void PrintInt32GenFiltered()
        {
            var gen = Gen.Int32().Where(x => x % 2 == 0);

            var print = PrintAndCollect(gen.Cast<object>());

            Snapshot.Match(print);
        }

        [Fact]
        public void PrintListOfInt32Gen()
        {
            var gen = Gen.Int32().ListOf();

            var print = PrintAndCollect(gen);

            Snapshot.Match(print);
        }

        [Fact]
        public void PrintNullaryProperty()
        {
            // TODO: This test should print "(no value)", rather than literal null (as nullary properties should be
            // rendered in this way. This is avoiding that rendering path, which isn't a big deal, but could cause
            // other issues / is a sign of hackiness.

            var property = Property.Nullary(() => false);

            var print = PrintAndCollect(property);

            Snapshot.Match(print);
        }

        [Fact]
        public void PrintUnaryProperty()
        {
            var property = Gen.Int32().ForAll((_) => false);

            var print = PrintAndCollect(property);

            Snapshot.Match(print);
        }

        [Fact]
        public void PrintBinaryProperty()
        {
            var property =
                from x in Gen.Int32()
                from y in Gen.Int32().GreaterThan(x)
                select Property.ForThese(() => false);

            var print = PrintAndCollect(property);

            Snapshot.Match(print);
        }

        [Fact]
        public void PrintUnaryListProperty()
        {
            var property = Gen.Int32().ListOf().ForAll((_) => false);

            var print = PrintAndCollect(property);

            Snapshot.Match(print);
        }

        [Fact]
        public void PrintBinaryListProperty()
        {
            var property =
                from xs in Gen.Int32().ListOf()
                from ys in Gen.Int32().ListOf().WithCountGreaterThan(xs.Count)
                select Property.ForThese(() => false);

            var print = PrintAndCollect(property);

            Snapshot.Match(print);
        }

        private static string PrintAndCollect(IGen<object> gen)
        {
            var log = new List<string>();

            gen.Advanced.Print(seed: 0, stdout: log.Add);

            return string.Join(Environment.NewLine, log);
        }
    }
}
