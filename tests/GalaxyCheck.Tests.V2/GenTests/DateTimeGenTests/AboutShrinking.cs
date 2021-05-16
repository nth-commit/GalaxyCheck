using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using NebulaCheck.Gens.Injection.Int32;
using System;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.DateTimeGenTests
{
    public class AboutShrinking
    {
        [Property]
        public void ItShrinksToTheMinimumDateTime(
            [Seed] int seed,
            [Date] DateTime dateTime)
        {
            var gen = GalaxyCheck.Gen.DateTime();

            var minimum = gen.Advanced.MinimumWithMetrics(seed: seed, pred: dt => dt >= dateTime);

            minimum.Should().Be(dateTime);
        }


        [Property]
        public void ItShrinksToTheMinimumYear(
            [Seed] int seed,
            [Between(1, 9999)] int year)
        {
            var gen = GalaxyCheck.Gen.DateTime();

            var minimum = gen.Minimum(seed: seed, pred: dateTime => dateTime.Year >= year);

            minimum.Should().Be(new DateTime(year: year, month: 1, day: 1));
        }

        [Property]
        public void ItShrinksToTheMinimumMonth(
            [Seed] int seed,
            [Between(1, 12)] int month)
        {
            var gen = GalaxyCheck.Gen.DateTime();

            var minimum = gen.Minimum(seed: seed, pred: dateTime => dateTime.Month >= month);

            minimum.Should().Be(new DateTime(year: 1, month: month, day: 1));
        }

        [Property]
        public void ItShrinksToTheMinimumDay(
            [Seed] int seed,
            [Between(1, 28)] int day)
        {
            var gen = GalaxyCheck.Gen.DateTime();

            var minimum = gen.Minimum(seed: seed, pred: dateTime => dateTime.Day >= day);

            minimum.Should().Be(new DateTime(year: 1, month: 1, day: day));
        }
    }
}
