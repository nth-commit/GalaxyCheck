using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.RunnerTests.CheckTests
{
    public class AboutIterations
    {
        [Property]
        public NebulaCheck.IGen<Test> IfThePropertyIsInfallible_ItCallsTheTestFunctionForEachIteration() =>
            from gen in DomainGen.Gen()
            from iterations in DomainGen.Iterations()
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var numberOfCalls = 0;
                var property = gen.ForAll((_) =>
                {
                    numberOfCalls++;
                    return true;
                });

                property.Check(iterations: iterations, seed: seed);

                numberOfCalls.Should().Be(iterations);
            });

        [Property]
        public NebulaCheck.IGen<Test> IfThePropertyIsInfallible_ItReturnsTheGivenIterations() =>
            from gen in DomainGen.Gen()
            from iterations in DomainGen.Iterations()
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
             {
                 var property = gen.ForAll((_) => true);

                 var result = property.Check(iterations: iterations, seed: seed);

                 result.Iterations.Should().Be(iterations);
             });
    }
}
