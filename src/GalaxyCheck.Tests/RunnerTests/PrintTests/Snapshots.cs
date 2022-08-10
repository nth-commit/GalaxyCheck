using GalaxyCheck;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.V2.RunnerTests.PrintTests
{
    public class Snapshots
    {
        public class PrintGen
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

            private static string PrintAndCollect<T>(IGen<T> gen)
            {
                var log = new List<string>();

                gen.Advanced.Print(seed: 0, stdout: log.Add);

                return string.Join(Environment.NewLine, log);
            }
        }

        public class PrintProperty
        {
            [Fact]
            public void PrintNullaryProperty()
            {
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
                var property = new Property(
                    from x in Gen.Int32().LessThan(int.MaxValue)
                    from y in Gen.Int32().GreaterThan(x)
                    select Property.ForThese(() => false));

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
                var property = new Property(
                    from xs in Gen.Int32().ListOf()
                    from ys in Gen.Int32().ListOf().WithCountGreaterThan(xs.Count)
                    select Property.ForThese(() => false));

                var print = PrintAndCollect(property);

                Snapshot.Match(print);
            }

            private static string PrintAndCollect(Property property)
            {
                var log = new List<string>();

                property.Print(seed: 0, stdout: log.Add);

                return string.Join(Environment.NewLine, log);
            }
        }

        public class PrintAsyncProperty
        {
            [Fact]
            public async Task PrintNullaryProperty()
            {
                var property = Property.NullaryAsync(() => Task.FromResult(false));

                var print = await PrintAndCollect(property);

                Snapshot.Match(print);
            }

            [Fact]
            public async Task PrintUnaryProperty()
            {
                var property = Gen.Int32().ForAllAsync((_) => Task.FromResult(false));

                var print = await PrintAndCollect(property);

                Snapshot.Match(print);
            }

            [Fact]
            public async Task PrintBinaryProperty()
            {
                var property = new AsyncProperty(
                    from x in Gen.Int32().LessThan(int.MaxValue)
                    from y in Gen.Int32().GreaterThan(x)
                    select Property.ForTheseAsync(() => Task.FromResult(false)));

                var print = await PrintAndCollect(property);

                Snapshot.Match(print);
            }

            [Fact]
            public async Task PrintUnaryListProperty()
            {
                var property = Gen.Int32().ListOf().ForAllAsync((_) => Task.FromResult(false));

                var print = await PrintAndCollect(property);

                Snapshot.Match(print);
            }

            [Fact]
            public async Task PrintBinaryListProperty()
            {
                var property = new AsyncProperty(
                    from xs in Gen.Int32().ListOf()
                    from ys in Gen.Int32().ListOf().WithCountGreaterThan(xs.Count)
                    select Property.ForTheseAsync(() => Task.FromResult(false)));

                var print = await PrintAndCollect(property);

                Snapshot.Match(print);
            }

            private static async Task<string> PrintAndCollect(AsyncProperty property)
            {
                var log = new List<string>();

                await property.PrintAsync(seed: 0, stdout: log.Add);

                return string.Join(Environment.NewLine, log);
            }
        }
    }
}
