using System;

namespace GalaxyCheck.Xunit
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PropertiesAttribute : Attribute
    {
        public Type? Factory { get; set; } = null;
    }
}
