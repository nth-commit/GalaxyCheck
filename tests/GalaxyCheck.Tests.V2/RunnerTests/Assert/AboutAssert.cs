using FluentAssertions;
using NebulaCheck;
using System;
using System.Linq;
using Xunit;
using Gen = NebulaCheck.Gen;

using GalaxyCheck;
using DevGen = GalaxyCheck.Gen;

namespace Tests.V2.RunnerTests.Assert
{

    public class AboutAssert
    {
        [Fact]
        public void ItCanPassForAVoidProperty() => Gen
            .ForAll(
                DomainGen.Iterations(),
                DomainGen.Seed(),
                DomainGen.Size(),
                (iterations, seed, size) =>
                {
                    Action action = () => DevGen
                        .Int32()
                        .ForAll(_ => { })
                        .Assert(iterations: 0, seed: seed, size: size);

                    action.Should().NotThrow();
                })
            .Assert();

        [Fact]
        public void ItCanPassForABooleanProperty() => Gen
            .ForAll(
                DomainGen.Iterations(),
                DomainGen.Seed(),
                DomainGen.Size(),
                (iterations, seed, size) =>
                {
                    Action action = () => DevGen
                        .Int32()
                        .ForAll(_ => true)
                        .Assert(iterations: iterations, seed: seed, size: size);

                    action.Should().NotThrow();
                })
            .Assert();

        [Fact]
        public void ItCanFailForAVoidProperty_FirstIteration() => Gen
            .ForAll(
                DomainGen.Iterations(),
                DomainGen.Seed(),
                DomainGen.Size(),
                (iterations, seed, size) =>
                {
                    Action action = () => DevGen
                        .Int32()
                        .ForAll(_ => Xunit.Assert.True(false))
                        .Assert(iterations: iterations, seed: seed, size: size);

                    action
                        .Should()
                        .Throw<GalaxyCheck.Runners.PropertyFailedException>()
                        .WithMessage($@"

                            Falsified after 1 test
                            Reproduction: (Seed = {seed}, Size = {size}, Path = new [] {{ }})
                            Counterexample: 0

                            ---- Assert.True() Failure
                            Expected: True
                            Actual:   False".TrimIndent());
                })
            .Assert();

        [Fact]
        public void ItCanFailForAVoidProperty_NthIteration() => Gen
            .ForAll(
                DomainGen.Iterations(minIterations: 2),
                DomainGen.Seed(),
                DomainGen.Size(),
                (iterations, seed, size) =>
                {
                    var iterationCounter = 0;
                    var hasFailed = false;

                    Action action = () => DevGen
                        .Int32()
                        .ForAll(_ =>
                        {
                            iterationCounter++;
                            if (iterationCounter >= iterations && !hasFailed)
                            {
                                hasFailed = true;
                                Xunit.Assert.True(false);
                            }
                        })
                        .Assert(iterations: iterations, seed: seed, size: size);

                    action
                        .Should()
                        .Throw<GalaxyCheck.Runners.PropertyFailedException>()
                        .WithMessage(@$"
                            
                            Falsified after {iterations} tests*".TrimIndent());
                })
            .Assert();

        [Fact]
        public void ItCanFailForABooleanProperty() => Gen
            .ForAll(
                DomainGen.Iterations(),
                DomainGen.Seed(),
                DomainGen.Size(),
                (iterations, seed, size) =>
                {
                    Action action = () => DevGen
                        .Int32()
                        .ForAll(_ => false)
                        .Assert(iterations: iterations, seed: seed, size: size);

                    action
                        .Should()
                        .Throw<GalaxyCheck.Runners.PropertyFailedException>()
                        .WithMessage($@"

                            Falsified after 1 test
                            Reproduction: (Seed = {seed}, Size = {size}, Path = new [] {{ }})
                            Counterexample: 0

                            Property function returned false".TrimIndent());
                })
            .Assert();

        [Fact]
        public void WhenIterationsIsZero_ItAlwaysPasses() => Gen
            .ForAll(
                DomainGen.Seed(),
                DomainGen.Size(),
                DomainGen.Boolean(),
                (seed, size, isFallible) =>
                {
                    Action action = () => DevGen
                        .Int32()
                        .ForAll(_ => isFallible)
                        .Assert(iterations: 0, seed: seed, size: size);

                    action.Should().NotThrow();
                })
            .Assert();
    }
}
