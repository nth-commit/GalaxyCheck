using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using NebulaCheck.Gens.Injection.Int32;
using System.Linq;
using System.Reflection;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.ParameterGenTests
{
    public class AboutOverridingDefaults
    {
        private static void MethodWithInt32Parameter(int parameter) { }

        [Property]
        public void ItGeneratesInt32sWithTheOverriddenGen([Seed] int seed, [Size] int size, int constantInt32Override)
        {
            var genFactory = GalaxyCheck.Gen
                .Factory()
                .RegisterType(GalaxyCheck.Gen.Constant(constantInt32Override));

            var gen = GalaxyCheck.Gen
                .Parameters(GetMethod(nameof(MethodWithInt32Parameter)), genFactory)
                .Select(x => x.Single())
                .Cast<int>();

            var sample = gen.SampleOne(seed: seed, size: size);

            sample.Should().Be(constantInt32Override);
        }

        private static void MethodWithInt32ParameterAndConstrainingAttribute([GalaxyCheck.Gens.Injection.Int32.LessThanEqual(-1)] int parameter) { }

        [Property]
        public void IfTheParameterHasAConstrainingAttribute_ItIgnoresTheOverrideAndTheConstrainingAttributeTakesPrecedent(
            [Seed] int seed,
            [Size] int size,
            [GreaterThanEqual(0)] int constantInt32Override)
        {
            var genFactory = GalaxyCheck.Gen
                .Factory()
                .RegisterType(GalaxyCheck.Gen.Constant(constantInt32Override));

            var gen = GalaxyCheck.Gen
                .Parameters(GetMethod(nameof(MethodWithInt32ParameterAndConstrainingAttribute)), genFactory)
                .Select(x => x.Single())
                .Cast<int>();

            var sample = gen.SampleOne(seed: seed, size: size);

            sample.Should().BeNegative();
        }

        private static MethodInfo GetMethod(string name)
        {
            return typeof(AboutOverridingDefaults).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;
        }
    }
}
