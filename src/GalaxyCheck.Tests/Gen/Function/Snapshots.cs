using Xunit;
using GC = GalaxyCheck;
using GalaxyCheck;
using static Tests.TestUtils;
using System.Linq;

namespace Tests.Gen.Function
{
    public class Snapshots
    {
        [Fact]
        public void Snapshot_Nullary_Int_Between_0_And_10()
        {
            var returnGen = GC.Gen.Int32().Between(1, 10);
            var gen = GC.Gen.Function(returnGen);

            SnapshotGenValues(gen, format: f => $"f() = {f()}");
        }

        [Fact]
        public void Snapshot_Unary_Int_Between_0_And_10()
        {
            var returnGen = GC.Gen.Int32().Between(0, 10);
            var gen = GC.Gen.Function<int, int>(returnGen);

            var inputs = Enumerable.Range(1, 8);

            SnapshotGenValues(gen, format: f => string.Join(", ", inputs.Select(x => $"f({x}) = {f(x)}")));
        }

        [Fact]
        public void Snapshot_Binary_Int_Between_0_And_10()
        {
            var returnGen = GC.Gen.Int32().Between(0, 10);
            var gen = GC.Gen.Function<int, int, int>(returnGen);

            var input = Enumerable.Range(1, 4);
            var inputs =
                from x in input
                from y in input
                select (x, y);

            SnapshotGenValues(gen, format: f => string.Join(", ", inputs.Select(args => $"f({args.x}, {args.y}) = {f(args.x, args.y)}")));
        }
    }
}
