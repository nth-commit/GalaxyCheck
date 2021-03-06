﻿using System;

namespace GalaxyCheck
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ReplayAttribute : Attribute
    {
        public string Replay { get; }

        public ReplayAttribute(string replay)
        {
            Replay = replay;
        }
    }
}
