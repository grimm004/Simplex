using System;
using System.Collections.Generic;
using System.Linq;

namespace Simplex
{
    class Constraint
    {
        public SortedDictionary<char, double> Variables { get; set; }
        public double Constant { get; set; }

        public Constraint()
        {
            Variables = new SortedDictionary<char, double>();
            Constant = 0;
        }

        public void VerifyVariables(char[] variables)
        {
            foreach (char variable in variables)
                if (!Variables.ContainsKey(variable)) Variables.Add(variable, 0);
        }

        public void AddVariable(char varName, double coefficient)
        {
            if (!Variables.TryAdd(varName, coefficient))
                Variables[varName] += coefficient;
        }

        public void SetConstant(double constant)
        {
            Constant = constant;
        }

        public void Output()
        {
            if (Variables.Count > 0)
            {
                List<KeyValuePair<char, double>> variables = Variables.ToList();
                for (int i = 0; i < variables.Count; i++)
                {
                    if (i == 0 && variables[i].Value < 0) Console.Write("-");
                    Console.Write($"{ (i > 0 ? (variables[i].Value < 0 ? " - " : " + ") : "") }");
                    if (variables[i].Value * variables[i].Value != 1) Console.Write($"{ (variables[i].Value * (variables[i].Value < 0 ? -1 : 1)) }");
                    Console.Write($"{ variables[i].Key }");
                }
                Console.WriteLine($" < { Constant }");
            }
            else Console.WriteLine("No Objective Function");
        }

        public Variable[] GetVars()
        {
            KeyValuePair<char, double>[] kvVars = Variables.ToArray();
            Variable[] vars = new Variable[kvVars.Length];

            for (int i = 0; i < kvVars.Length; i++)
                vars[i] = new Variable { Index = i, Placeholder = kvVars[i].Key, Value = kvVars[i].Value };

            return vars;
        }
    }
}
