namespace GalaxyCheck
{
    using System;

    public partial class Property
    {
        public static void Precondition(bool condition)
        {
            if (!condition)
            {
                throw new PropertyPreconditionException();
            }
        }

        public class PropertyPreconditionException : Exception
        {
        }
    }
}
