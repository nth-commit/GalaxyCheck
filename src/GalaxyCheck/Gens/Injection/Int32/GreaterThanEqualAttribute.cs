﻿using System;

namespace GalaxyCheck.Gens.Injection.Int32
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class GreaterThanEqualAttribute : GenAttribute
    {
        public int Min { get; }

        public GreaterThanEqualAttribute(int min)
        {
            Min = min;
        }

        public override IGen Value => GalaxyCheck.Gen.Int32().GreaterThanEqual(Min);
    }
}
