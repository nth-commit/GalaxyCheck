using FsCheck;
using Microsoft.FSharp.Core;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public record Iterations(int Value);

    public static class ArbitraryIterations
    {
        private static readonly FSharpFunc<Iterations, IEnumerable<Iterations>> Shrinker =
            FuncConvert.FromFunc<Iterations, IEnumerable<Iterations>>(iterations => Arb.Shrink(iterations.Value).Select(x => new Iterations(x)));

        public static Arbitrary<Iterations> Iterations() => FsCheck.Gen.Choose(0, 100).Select(x => new Iterations(x)).ToArbitrary(Shrinker);

    }
}
