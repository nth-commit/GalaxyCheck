using System;

namespace GalaxyCheck
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class MemberGenAttribute : Attribute
    {
        public string MemberName { get; }

        public MemberGenAttribute(string memberName)
        {
            MemberName = memberName;
        }
    }
}
