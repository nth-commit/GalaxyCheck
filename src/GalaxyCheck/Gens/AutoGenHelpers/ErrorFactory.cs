namespace GalaxyCheck.Gens.AutoGenHelpers
{
    internal delegate IGen ErrorFactory(string message);

    internal delegate IGen ContextualErrorFactory(string message, AutoGenHandlerContext context);
}
