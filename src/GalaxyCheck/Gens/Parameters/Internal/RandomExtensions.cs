using System;

namespace GalaxyCheck.Gens.Parameters.Internal
{
    internal static class RandomExtensions
    {
        public static long NextLong(this Random random, long min, long max)
        {
            if (min == max)
            {
                return min;
            }

            ulong uRange = (ulong)(max - min);
            ulong ulongRand;
            do
            {
                ulongRand = random.NextUnsignedLong();
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + min;
        }

        private static ulong NextUnsignedLong(this Random random)
        {
            byte[] buf = new byte[8];
            random.NextBytes(buf);
            return (ulong)BitConverter.ToInt64(buf, 0);
        }
    }
}