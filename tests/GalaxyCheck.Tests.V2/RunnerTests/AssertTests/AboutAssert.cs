using FluentAssertions;
using NebulaCheck;
using NebulaCheck.Xunit;
using Property = NebulaCheck.Property;
using System;
using System.Linq;
using Xunit;
using GalaxyCheck;
using DevGen = GalaxyCheck.Gen;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.RunnerTests.AssertTests
{
    public class AboutAssert
    {
        [Property]
        public void ItCanPassForAVoidProperty(
            [Iterations] int iterations,
            [Seed] int seed,
            [Size] int size)
        {
            Action action = () => DevGen
                .Int32()
                .ForAll(_ => { })
                .Assert(iterations: iterations, seed: seed, size: size);

            action.Should().NotThrow();
        }

        [Property]
        public void ItCanFailForAVoidProperty_FirstIteration(
            [Iterations] int iterations,
            [Seed] int seed,
            [Size] int size)
        {
            Action action = () => DevGen
                .Int32()
                .ForAll(_ => Assert.True(false))
                .Assert(iterations: iterations, seed: seed, size: size);

            action.Should().Throw<GalaxyCheck.Runners.PropertyFailedException>();
        }

        [Property]
        public void ItCanFailForAVoidProperty_NthIteration(
            [Iterations(minIterations: 2)] int iterations,
            [Seed] int seed,
            [Size] int size)
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
                        Assert.True(false);
                    }
                })
                .Assert(iterations: iterations, seed: seed, size: size);

            action.Should().Throw<GalaxyCheck.Runners.PropertyFailedException>();
        }

        [Property]
        public void ItCanPassForABooleanProperty(
            [Iterations] int iterations,
            [Seed] int seed,
            [Size] int size)
        {
            Action action = () => DevGen
                .Int32()
                .ForAll(_ => true)
                .Assert(iterations: iterations, seed: seed, size: size);

            action.Should().NotThrow();
        }

        [Property]
        public void ItCanFailForABooleanProperty(
            [Iterations] int iterations,
            [Seed] int seed,
            [Size] int size)
        {
            Action action = () => DevGen
                .Int32()
                .ForAll(_ => false)
                .Assert(iterations: iterations, seed: seed, size: size);

            action.Should().Throw<GalaxyCheck.Runners.PropertyFailedException>();
        }

        [Property]
        public Property WhenIterationsIsZero_ItPasses(
            [Seed] int seed,
            [Size] int size)
        {
            // TODO: Inject bool when it is supported natively

            return DomainGen.Boolean().ForAll(isPropertyFallible =>
            {
                Action action = () => DevGen
                    .Int32()
                    .ForAll(_ => isPropertyFallible)
                    .Assert(iterations: 0, seed: seed, size: size);

                action.Should().NotThrow();
            });
        }
    }
}
