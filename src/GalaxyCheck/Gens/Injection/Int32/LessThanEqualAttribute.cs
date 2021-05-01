﻿using System;

namespace GalaxyCheck.Gens.Injection.Int32
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class LessThanEqualAttribute : GenProviderAttribute
    {
        public int Max { get; }


        public LessThanEqualAttribute(int max)
        {
            Max = max;
        }

        public override IGen Get => GalaxyCheck.Gen.Int32().LessThanEqual(Max);
    }
}
