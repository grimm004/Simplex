using System;

namespace Simplex
{
    class Program
    {
        static void Main(string[] args)
        {
            LinearProgram lp = Example1();
            lp.Output();
            new Simplex(lp).Solve();
            Console.ReadKey();
        }

        static LinearProgram Example1()
        {
            LPParser parser = new LPParser();
            parser.SetObjectiveFunction("8x + 10y + 7z");
            parser.AddConstraint("x + 3y + 2z < 10");
            parser.AddConstraint("x + 5y + z < 8");
            return parser.LinearProgram;
        }

        static LinearProgram Example2()
        {
            LPParser parser = new LPParser();
            parser.SetObjectiveFunction("x + 2y - z");
            parser.AddConstraint("2x + y + z < 14");
            parser.AddConstraint("4x + 2y + 3z < 28");
            parser.AddConstraint("2x + 5y + 5z < 30");
            return parser.LinearProgram;
        }

        static LinearProgram Example3()
        {
            LPParser parser = new LPParser();
            parser.SetObjectiveFunction("3x + 4y");
            parser.AddConstraint("x + y < 4");
            parser.AddConstraint("2x + y < 5");
            return parser.LinearProgram;
        }

        static LinearProgram Example4()
        {
            // TODO: FIX NEGITIVE ENTRIES
            LPParser parser = new LPParser();
            parser.SetObjectiveFunction("-2x + y");
            parser.AddConstraint("x + 2y < 6");
            parser.AddConstraint("3x + 2y < 12");
            return parser.LinearProgram;
        }
    }
}
