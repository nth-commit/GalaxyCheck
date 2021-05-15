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

                return Gen
                    .Int64()
                    .Between(minDateTime.Ticks, maxDateTime.Ticks)
                    .Select(ticks => new DateTime(ticks, DateTimeKind.Unspecified));
            }
        }

        private static IGen<DateTime> Error(string message) =>
            Gen.Advanced.Error<DateTime>(nameof(DateTimeGen), message);
    }
}
