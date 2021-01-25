using System.Linq;

namespace GalaxyCheck.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var gen = Gen
                .Int32()
                .Between(0, 10)
                .ListOf();

            //var property = gen.ForAll(_ => false);
            var property = gen.ForAll(xs => xs.Sum() < 11);

            property.Check(iterations: 100, seed: 0);

            property.Print(iterations: 100, size: 100);
        }
    }
}
