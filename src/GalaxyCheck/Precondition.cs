using System.Collections.Generic;
using System.Text;

namespace GalaxyCheck
{
    using GalaxyCheck.Properties;

    public partial class Property
    {
        public static void Precondition(bool condition)
        {
            if (!condition)
            {
                throw new PropertyPreconditionException();
            }
        }
    }
}

namespace GalaxyCheck.Properties
{
    using System;

    public class PropertyPreconditionException : Exception
    {
    }
}
