﻿using System;
using SimplexMethod;

namespace SimpleCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            LinearProgram[] linearProgrammes = new LinearProgram[6];

            LPParser parser = new LPParser();
            parser.SetObjectiveFunction("8x + 10y + 7z");
            parser.AddConstraint("x + 3y + 2z < 10");
            parser.AddConstraint("x + 5y + z < 8");
            linearProgrammes[0] = parser.LinearProgram;

            parser = new LPParser();
            parser.SetObjectiveFunction("x + 2y - z");
            parser.AddConstraint("2x + y + z < 14");
            parser.AddConstraint("4x + 2y + 3z < 28");
            parser.AddConstraint("2x + 5y + 5z < 30");
            linearProgrammes[1] = parser.LinearProgram;

            parser = new LPParser();
            parser.SetObjectiveFunction("3x + 4y");
            parser.AddConstraint("x + y < 4");
            parser.AddConstraint("2x + y < 5");
            linearProgrammes[2] = parser.LinearProgram;

            parser = new LPParser();
            parser.SetObjectiveFunction("-2x + y");
            parser.AddConstraint("x + 2y < 6");
            parser.AddConstraint("3x + 2y < 12");
            linearProgrammes[3] = parser.LinearProgram;

            parser = new LPParser();
            parser.SetObjectiveFunction("2x + 3y");
            parser.AddConstraint("x + y < 2000");
            parser.AddConstraint("8x + 14y < 20000");
            linearProgrammes[4] = parser.LinearProgram;

            parser = new LPParser();
            parser.SetObjectiveFunction("x + 0.8y");
            parser.AddConstraint("x + y < 1000");
            parser.AddConstraint("2x + y < 1500");
            parser.AddConstraint("3x + 2y < 2400");
            linearProgrammes[5] = parser.LinearProgram;

            for (int i = 0; i < linearProgrammes.Length; i++)
            {
                Console.WriteLine("----------------------------------");
                Console.WriteLine($"LP { i + 1 }");
                linearProgrammes[i].Output();
                Console.WriteLine();
                Console.WriteLine("Solution:");
                new Simplex(linearProgrammes[i]).Solve().Output();
            }
            Console.WriteLine("----------------------------------");
            Console.ReadKey();
        }
    }
}
