using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Gens.Parameters.Internal
{
    internal class SamplingSize
    {
        public static IEnumerable<Size> SampleSize(int numberOfSamples)
        {
            var clampedNumberOfSamples = Math.Min(numberOfSamples, 101);
            var interpolate = Interpolate(0, 0, clampedNumberOfSamples - 1, 100);

            return Enumerable
                .Range(0, clampedNumberOfSamples)
                .Select(x => x == clampedNumberOfSamples - 1
                    ? new Size(100)
                    : new Size(interpolate(x)));
        }

        private static Func<int, int> Interpolate(int x1, int y1, int x2, int y2) => (x) =>
        {
            return (int)(y1 + (x - x1) * ((double)(y2 - y1) / (x2 - x1)));
        };
    }
}
