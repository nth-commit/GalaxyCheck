namespace GalaxyCheck.Gens.ReflectedGenHelpers
{
    internal delegate IGen ErrorFactory(string message);

    internal delegate IGen ContextualErrorFactory(string message, ReflectedGenHandlerContext context);
}
