using Xunit;
using GalaxyCheck;
using NebulaCheck;
using System;
using DevGen = GalaxyCheck.Gen;
using Gen = NebulaCheck.Gen;
using FluentAssertions;
using System.Linq;

namespace Tests.V2.RunnerTests.Assert
{

    public class AboutAssert
    {
        [Fact]
        public void ItCanPassForAVoidProperty()
        {
            var check = Gen.ForAll(
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
                .Check();

            check.Counterexample.Should().BeNull();
        }

        [Fact]
        public void ItCanPassForABooleanProperty()
        {
            var check = Gen.ForAll(
                DomainGen.Iterations(),
                DomainGen.Seed(),
                DomainGen.Size(),
                (iterations, seed, size) =>
                {
                    Action action = () => DevGen
                        .Int32()
                        .ForAll(_ => true)
                        .Assert(iterations: 0, seed: seed, size: size);

                    action.Should().NotThrow();
                })
                .Check();

            check.Counterexample.Should().BeNull();
        }

        [Fact]
        public void ItCanFailForAVoidProperty_FirstIteration()
        {
            var check = Gen.ForAll(
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
                        .Throw<GalaxyCheck.Runners.PropertyFailedException<int>>()
                        .WithMessage($@"
                            Falsified after 1 test
                            Reproduction: (Seed = {seed}, Size = {size}, Path = new [] {{ }})
                            Counterexample: 0

                            ---- Assert.True() Failure
                            Expected: True
                            Actual:   False".TrimIndent());
                })
                .Check();

            Xunit.Assert.Null(check.Counterexample);
        }

        [Fact]
        public void ItCanFailForAVoidProperty_NthIteration()
        {
            var check = Gen.ForAll(
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
                        .Throw<GalaxyCheck.Runners.PropertyFailedException<int>>()
                        .WithMessage($"Falsified after {iterations} tests*");
                })
                .Check();

            Xunit.Assert.Null(check.Counterexample);
        }

        [Fact]
        public void ItCanFailForABooleanProperty()
        {
            var check = Gen.ForAll(
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
                        .Throw<GalaxyCheck.Runners.PropertyFailedException<int>>()
                        .WithMessage($@"
                            Falsified after 1 test
                            Reproduction: (Seed = {seed}, Size = {size}, Path = new [] {{ }})
                            Counterexample: 0

                            Property function returned false".TrimIndent());
                })
                .Check();

            Xunit.Assert.Null(check.Counterexample);
        }

        [Fact]
        public void WhenIterationsIsZero_ItAlwaysPasses()
        {
            var check = Gen.ForAll(
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
                .Check();

            check.Counterexample.Should().BeNull();
        }
    }
}
