﻿using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Tests.V2.DomainGenAttributes;
using static Tests.V2.Timeouts;

namespace Tests.V2.RunnerTests.AssertTests
{
    public class AboutExhaustion
    {
        public class Sync
        {
            [Property(Iterations = 1)]
            public void ItCanExhaust([Seed] int seed, [Size] int size)
            {
                RunWithTimeout(
                    () =>
                    {
                        var property = GalaxyCheck.Gen.Int32().Where(x => false).ForAll(_ => true);

                        Action test = () => property.Assert(seed: seed, size: size, deepCheck: false);

                        test.Should().Throw<GalaxyCheck.Exceptions.GenExhaustionException>();
                    },
                    TimeSpan.FromSeconds(60));
            }
        }

        public class Async
        {
            [Property(Iterations = 1)]
            public void ItCanExhaustAsync([Seed] int seed, [Size] int size)
            {
                RunWithTimeoutAsync(
                    async () =>
                    {
                        var property = GalaxyCheck.Gen.Int32().Where(x => false).ForAllAsync(_ => Task.FromResult(true));

                        Func<Task> test = () => property.AssertAsync(seed: seed, size: size, deepCheck: false);

                        await test.Should().ThrowAsync<GalaxyCheck.Exceptions.GenExhaustionException>();
                    },
                    TimeSpan.FromSeconds(60));
            }
        }
    }
}
