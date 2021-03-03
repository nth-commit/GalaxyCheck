using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using NebulaCheck.Injection.Int32;

namespace Tests.V2
{
    public static class DomainGen
    {
        public static NebulaCheck.IGen<T> Choose<T>(params NebulaCheck.IGen<T>[] gens)
        {
            if (gens.Any() == false)
            {
                return new NebulaCheck.Internal.Gens.ErrorGen<T>("DomainGen.Choose", "Input gens was empty");
            }

            return NebulaCheck.Gen.Int32()
                .WithBias(NebulaCheck.Gen.Bias.None)
                .Between(0, gens.Length - 1)
                .SelectMany(i => gens[i]);
        }

        public static NebulaCheck.IGen<GalaxyCheck.IGen<object>> Gen() =>
            Choose<GalaxyCheck.IGen>(
                NebulaCheck.Gen.Constant(GalaxyCheck.Gen.Int32()),
                NebulaCheck.Gen.Constant(GalaxyCheck.Gen.Int32().Between(0, 1).WithBias(GalaxyCheck.Gen.Bias.None))
            ).Select(gen => gen.Cast<object>());

        public static NebulaCheck.IGen<bool> Boolean() =>
            from x in NebulaCheck.Gen.Int32().Between(0, 1).WithBias(NebulaCheck.Gen.Bias.None)
            select (x == 1);

        
    }
}
