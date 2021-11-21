using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using NebulaCheck.Gens.Injection.Int32;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.ParameterGenTests
{
    public class AboutCustomGens
    {
        public class AboutValidation
        {
            private static void MethodWithOneParameter(int x) { }

            [Property]
            public void ItErrorsIfParameterIndexIsNotOfTheMethod([GreaterThanEqual(1)] int i, [Seed] int seed)
            {
                var gen = GalaxyCheck.Gen.Parameters(
                    GetMethod(nameof(MethodWithOneParameter)),
                    customGens: new Dictionary<int, GalaxyCheck.IGen>
                    {
                        { i, GalaxyCheck.Gen.Int32() }
                    });

                GenAssert.Errors(
                    gen,
                    seed: seed,
                    expectedMessage: $"Error while running generator ParametersGen: parameter index '{i}' of custom generator was not valid");
            }

            [Property]
            public void ItErrorsIfTheTypeOfTheGeneratorIsNotAssignableToTheTypeOfTheParameter([Seed] int seed)
            {
                var gen = GalaxyCheck.Gen.Parameters(
                    GetMethod(nameof(MethodWithOneParameter)),
                    customGens: new Dictionary<int, GalaxyCheck.IGen>
                    {
                        { 0, GalaxyCheck.Gen.String() }
                    });

                GenAssert.Errors(
                    gen,
                    seed: seed,
                    expectedMessage: $"Error while running generator ParametersGen: generator of type 'System.String' is not compatible with parameter of type 'System.Int32'");
            }

            private static MethodInfo GetMethod(string name)
            {
                return typeof(AboutValidation).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;
            }
        }

        public class AboutValueProduction
        {
            private static void MethodWithOneParameter(int x) { }

            [Property]
            public void ItProducesValuesUsingTheCustomGenerator([Seed] int seed, [Size] int size, int minimumValue)
            {
                var gen1 = GalaxyCheck.Gen.Int32().GreaterThanEqual(minimumValue);

                var gen0 = GalaxyCheck.Gen
                    .Parameters(
                        GetMethod(nameof(MethodWithOneParameter)),
                        customGens: new Dictionary<int, GalaxyCheck.IGen>
                        {
                            { 0, gen1 }
                        })
                    .Select(parameters => (int)parameters.Single());

                var sample0 = gen0.SampleOneTraversal(seed: seed, size: size);
                var sample1 = gen1.SampleOneTraversal(seed: seed, size: size);

                sample0.Should().BeEquivalentTo(sample1);
            }

            private static MethodInfo GetMethod(string name)
            {
                return typeof(AboutValueProduction).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;
            }
        }
    }
}
