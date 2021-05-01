namespace GalaxyCheck.Gens.AutoGenHelpers
{
    internal delegate IGen ErrorFactory(string message, object error);

    internal delegate IGen ContextualErrorFactory(string message, object error, AutoGenHandlerContext context);
}
