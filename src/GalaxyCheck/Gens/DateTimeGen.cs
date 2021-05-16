using GalaxyCheck.Gens;
using GalaxyCheck.Gens.Internal;
using System;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        /// <summary>
        /// Creates a generator that produces datetimes. By default, it will generate values in the full range
        /// (<see cref="System.DateTime.MinValue"/> to <see cref="System.DateTime.MaxValue"/>), but the generator
        /// returned contains configuration methods to constrain the produced values further.
        /// </summary>
        /// <returns>The new generator.</returns>
        public static IDateTimeGen DateTime() => new DateTimeGen(
            MinDateTime: null,
            MaxDateTime: null);
    }

    public static partial class Extensions
    {
        public static IDateTimeGen After(this IDateTimeGen gen)
        {
            throw new NotImplementedException();
        }

        public static IDateTimeGen Before(this IDateTimeGen gen)
        {
            throw new NotImplementedException();
        }

        public static IDateTimeGen Within(this IDateTimeGen gen, DateTime x, DateTime y)
        {
            throw new NotImplementedException();
        }
    }
}

namespace GalaxyCheck.Gens
{
    public interface IDateTimeGen : IGen<DateTime>
    {
        /// <summary>
        /// Constrains the generator so that it only produces values after the given datetime.
        /// </summary>
        /// <param name="dateTime">The minimum (inclusive) datetime to generate.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IDateTimeGen From(DateTime dateTime);

        /// <summary>
        /// Constrains the generator so that it only produces values before the given datetime.
        /// </summary>
        /// <param name="dateTime">The maximum (inclusive) datetime to generate.</param>
        /// <returns>A new generator with the constraint applied.</returns>
        IDateTimeGen To(DateTime dateTime);
    }

    internal record DateTimeGen(
        DateTime? MinDateTime = null,
        DateTime? MaxDateTime = null) : GenProvider<DateTime>, IDateTimeGen
    {
        public IDateTimeGen From(DateTime dateTime) => this with { MinDateTime = dateTime };

        public IDateTimeGen To(DateTime dateTime) => this with { MaxDateTime = dateTime };

        protected override IGen<DateTime> Get
        {
            get
            {
                var minDateTime = MinDateTime ?? DateTime.MinValue;
                var maxDateTime = MaxDateTime ?? DateTime.MaxValue;

                if (minDateTime > maxDateTime)
                {
                    return Error("'from' datetime cannot be after 'to' datetime");
                }

                return
                    from date in DateGen(minDateTime, maxDateTime)
                    from time in TimeGen(minDateTime, maxDateTime, date)
                    select date.Add(time);
            }
        }

        private static IGen<DateTime> DateGen(DateTime minDateTime, DateTime maxDateTime) =>
            from year in Gen.Int32().Between(minDateTime.Year, maxDateTime.Year)
            from month in MonthGen(minDateTime, maxDateTime, year)
            from day in DayGen(minDateTime, maxDateTime, year, month)
            select new DateTime(
                year: year,
                month: month,
                day: day,
                hour: 0,
                minute: 0,
                second: 0,
                kind: DateTimeKind.Unspecified);

        private static IGen<int> MonthGen(DateTime minDateTime, DateTime maxDateTime, int year)
        {
            var shouldConstrainMinMonth = year == minDateTime.Year;
            var minMonth = shouldConstrainMinMonth ? minDateTime.Month : 1;

            var shoulConstrainMaxMonth = year == maxDateTime.Year;
            var maxMonth = shoulConstrainMaxMonth ? maxDateTime.Month : 12;

            return Gen.Int32().Between(minMonth, maxMonth);
        }

        private static IGen<int> DayGen(DateTime minDateTime, DateTime maxDateTime, int year, int month)
        {
            var shouldConstrainMinDay = year == minDateTime.Year && month == minDateTime.Month;
            var minDay = shouldConstrainMinDay ? minDateTime.Day : 1;

            var shouldConstrainMaxDay = year == maxDateTime.Year && month == maxDateTime.Month;
            var daysInMonth = DateTime.DaysInMonth(year: year, month: month);
            var maxDay = shouldConstrainMaxDay ? maxDateTime.Day : daysInMonth;

            return Gen.Int32().Between(minDay, maxDay);
        }

        private static IGen<TimeSpan> TimeGen(DateTime minDateTime, DateTime maxDateTime, DateTime date)
        {
            // It's much more digestible, and probably a solid perf shortcut (reduces number of shrinks), if we just
            // generate whole seconds. However, we might need to generate dates that have significant ticks - for
            // example, if the minimum date and maximum date have literally a single tick between them. Let's just
            // roughly approximate if we might need ticks. If neither of the constraints are the date we generated,
            // then ticks vs. seconds is ineffectual in terms of violating either of those constraints. There's a
            // much more precise way to distinguish if ticks matter, but this is good enough for now.

            var timeRequiresTicksResolution = minDateTime.Date == date.Date || maxDateTime.Date == date.Date;
            return timeRequiresTicksResolution
                ? TimeGenWithTicksResolution(minDateTime, maxDateTime, date)
                : TimeGenWithSecondsResolution(minDateTime, maxDateTime, date);
        }

        private static IGen<TimeSpan> TimeGenWithTicksResolution(DateTime minDateTime, DateTime maxDateTime, DateTime date)
        {
            var shouldConstrainMinTime = minDateTime.Date == date;
            var minTicks = shouldConstrainMinTime ? minDateTime.TimeOfDay.Ticks : 0;

            var shouldConstrainMaxTime = maxDateTime.Date == date;
            var ticksInDay = TimeSpan.FromHours(24).Ticks - 1;
            var maxTicks = shouldConstrainMaxTime ? maxDateTime.TimeOfDay.Ticks : ticksInDay;

            return Gen.Int64().Between(minTicks, maxTicks).Select(ticks => TimeSpan.FromTicks(ticks));
        }

        private static IGen<TimeSpan> TimeGenWithSecondsResolution(DateTime minDateTime, DateTime maxDateTime, DateTime date)
        {
            var shouldConstrainMinTime = minDateTime.Date == date;
            var minSeconds = shouldConstrainMinTime ? (int)minDateTime.TimeOfDay.TotalSeconds : 0;

            var shouldConstrainMaxTime = maxDateTime.Date == date;
            var ticksInDay = (int)(TimeSpan.FromHours(24).TotalSeconds - 1);
            var maxSeconds = shouldConstrainMaxTime ? (int)maxDateTime.TimeOfDay.TotalSeconds : ticksInDay;

            return Gen
                .Int32()
                .Between(minSeconds, maxSeconds)
                .Select(seconds => TimeSpan.FromSeconds(seconds));
        }

        private static IGen<DateTime> Error(string message) =>
            Gen.Advanced.Error<DateTime>(nameof(DateTimeGen), message);
    }
}
