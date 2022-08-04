using System.Linq;

namespace GalaxyCheck.Sandbox
{
    class Program
    {
        private class ClassWith10Members
        {
            public int Property0 { get; set; }
            public int Property1 { get; set; }
            public int Property2 { get; set; }
            public int Property3 { get; set; }
            public int Property4 { get; set; }
            public int Property5 { get; set; }
            public int Property6 { get; set; }
            public int Property7 { get; set; }
            public int Property8 { get; set; }
            public int Property9 { get; set; }
        }

        static void Main(string[] args)
        {
            var gen = Gen.Create<ClassWith10Members>();

            gen.Sample(iterations: 100, seed: 0);
        }
    }
}
