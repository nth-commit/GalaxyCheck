using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        /// <summary>
        /// Generates elements picked from the source enumerable.
        /// </summary>
        /// <param name="source">The source elements to pick from.</param>
        /// <returns>A new generator.</returns>
        public static IGen<T> Element<T>(IEnumerable<T> source)
        {
            var sourceList = source.ToList();

            if (sourceList.Count == 0)
            {
                return Advanced.Error<T>(nameof(Element), "'source' must not be empty");
            }

            return Int32().Between(0, sourceList.Count - 1).Select(i => sourceList[i]);
        }
    }
}
