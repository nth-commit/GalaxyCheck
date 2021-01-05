using GalaxyCheck.Abstractions;
using System;

namespace GalaxyCheck.Sizing
{
    public record Size : ISize
    {
        public static readonly Size MinValue = new Size(0);

        public static readonly Size MaxValue = new Size(100);

        public int Value { get; init; }

        public Size(int value)
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "'value' must be between 0 and 100");
            }

            Value = value;
        }
    }
}
