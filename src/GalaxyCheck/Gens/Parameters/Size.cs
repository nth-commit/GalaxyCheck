﻿using System;

namespace GalaxyCheck.Gens.Parameters
{
    public record Size
    {
        public Size(int value)
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "'value' must be between 0 and 100");
            }

            Value = value;
        }

        public int Value { get; init; }

        public Size Increment() => new Size((Value + 1) % 100);

        public Size BigIncrement() => new Size((Value + 5) % 100);
    }
}
