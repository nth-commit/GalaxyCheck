using System;

namespace GalaxyCheck
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PropertiesAttribute : Attribute
    {
        public Type? Factory { get; set; } = null;
    }
}
