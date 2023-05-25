using GalaxyCheck.Gens.Parameters;

namespace GalaxyCheck.Tests.TestUtility;

public class DummyGens
{
    public static IGen<object> Object() => Gen.Int32().Cast<object>();

    public static IGen<GenParameters> TheInputGenParameters() => Gen.Create(parameters => (value: parameters, nextParameters: parameters));

    public static IGen<Size> TheInputSize() => TheInputGenParameters().Select(it => it.Size);
}
