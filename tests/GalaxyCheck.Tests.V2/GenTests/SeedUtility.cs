using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.V2.GenTests
{
    public static class SeedUtility
    {
        public static int Fork(int seed) => GalaxyCheck.Gens.Parameters.Internal.Rng.Create(seed).Fork().Seed;

        public static int Skip(int seed) => GalaxyCheck.Gens.Parameters.Internal.Rng.Create(seed).Next().Seed;
    }
}
