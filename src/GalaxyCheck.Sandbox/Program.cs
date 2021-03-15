using System.Linq;

namespace GalaxyCheck.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var returnGen = Gen.Int32().Between(0, 100);
            var gen = Gen.Function<int, int>(returnGen);

            var func = gen.SampleOne(seed: 0);

            var xs0 = Enumerable.Range(0, 10).Select(func).ToList();
            var xs1 = Enumerable.Range(0, 10).Select(func).ToList();

            if (xs0.SequenceEqual(xs1) == false)
            {
                throw new System.Exception("Not a pure func");
            }
        }
    }
}