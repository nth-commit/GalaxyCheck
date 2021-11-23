using System;
using System.Linq;

namespace Tests.V2
{
    public static class StringExtensions
    {
        public static string TrimIndent(this string str)
        {
            var lines = str.Split(Environment.NewLine).Select(line => line.TrimStart());

            if (lines.First() == "")
            {
                return string.Join(Environment.NewLine, lines.Skip(1));
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
