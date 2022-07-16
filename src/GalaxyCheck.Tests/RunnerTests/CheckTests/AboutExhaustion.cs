using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Tests.V2.DomainGenAttributes;
using static Tests.V2.Timeouts;

namespace Tests.V2.RunnerTests.CheckTests
{
    public class AboutExhaustion
    {
        public class Sync
        {
            [Property(Iterations = 1)]
            public void ItExhaustsWhenGenerationIsImpossible([Seed] int seed, [Size] int size)
            {
                RunWithTimeout(
                    () =>
                    {
                        var property = GalaxyCheck.Gen.Int32().Where(x => false).ForAll(_ => true);

                        Action test = () => property.Check(seed: seed, size: size);

                        test.Should().Throw<GalaxyCheck.Exceptions.GenExhaustionException>();

                    },
                    TimeSpan.FromSeconds(60));
            }

            [Property(Iterations = 1)]
            public void ItExhaustsWhenPreconditionIsImpossible([Seed] int seed, [Size] int size)
            {
                RunWithTimeout(
                    () =>
                    {
                        var property = GalaxyCheck.Gen.Int32().ForAll(_ => GalaxyCheck.Property.Precondition(false));

                        Action test = () => property.Check(seed: seed, size: size);

                        test.Should().Throw<GalaxyCheck.Exceptions.GenExhaustionException>();
                    },
                    TimeSpan.FromSeconds(60));
            }
        }

        public class Async
        {
            [Property(Iterations = 1)]
            public void ItExhaustsWhenGenerationIsImpossible([Seed] int seed, [Size] int size)
            {
                RunWithTimeoutAsync(
                    async () =>
                    {
                        var property = GalaxyCheck.Gen.Int32().Where(x => false).ForAllAsync(_ => Task.CompletedTask);

                        Func<Task> test = () => property.CheckAsync(seed: seed, size: size);

                        await test.Should().ThrowAsync<GalaxyCheck.Exceptions.GenExhaustionException>();

                    },
                    TimeSpan.FromSeconds(60));
            }

            [Property(Iterations = 1)]
            public void ItExhaustsWhenPreconditionIsImpossible([Seed] int seed, [Size] int size)
            {
                RunWithTimeoutAsync(
                    async () =>
                    {
                        var property = GalaxyCheck.Gen.Int32().ForAllAsync(async _ =>
                        {
                            await Task.CompletedTask;
                            GalaxyCheck.Property.Precondition(false);
                        });

                        Func<Task> test = () => property.CheckAsync(seed: seed, size: size);

                        await test.Should().ThrowAsync<GalaxyCheck.Exceptions.GenExhaustionException>();
                    },
                    TimeSpan.FromSeconds(60));
            }

        }
    }
}
