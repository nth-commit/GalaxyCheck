using FsCheck;
using Microsoft.FSharp.Core;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public record Iterations(int Value);

    public record NonZeroIterations(int Value);

    public static class ArbitraryIterations
    {
        private static readonly FSharpFunc<Iterations, IEnumerable<Iterations>> IterationsShrinker =
            FuncConvert.FromFunc<Iterations, IEnumerable<Iterations>>(iterations => Arb.Shrink(iterations.Value).Select(x => new Iterations(x)));

        public static Arbitrary<Iterations> Iterations() => FsCheck.Gen.Choose(0, 100).Select(x => new Iterations(x)).ToArbitrary(IterationsShrinker);

        private static readonly FSharpFunc<NonZeroIterations, IEnumerable<NonZeroIterations>> NonZeroIterationsShrinker =
            FuncConvert.FromFunc<NonZeroIterations, IEnumerable<NonZeroIterations>>(iterations => Arb.Shrink(iterations.Value).Select(x => new NonZeroIterations(x)));

        public static Arbitrary<NonZeroIterations> NonZeroIterations() => FsCheck.Gen.Choose(1, 100).Select(x => new NonZeroIterations(x)).ToArbitrary(NonZeroIterationsShrinker);
    }
}
