using FsCheck;

namespace Tests
{
    public record Size(int Value);

    public static class ArbitrarySize
    {

        public static Arbitrary<Size> Size() => FsCheck.Gen.Choose(0, 100).Select(x => new Size(x)).ToArbitrary();

    }
}
