using System;

namespace Simplex
{
    class Program
    {
        static void Main(string[] args)
        {
            LPParser parser = new LPParser();
            parser.SetObjectiveFunction("8x + 10y + 7z");
            parser.AddConstraint("x + 3y + 2z < 10");
            parser.AddConstraint("x + 5y + z < 8");
            parser.LinearProgram.Output();
            new Simplex(parser.LinearProgram).Solve();
            Console.ReadKey();
        }
    }
}
