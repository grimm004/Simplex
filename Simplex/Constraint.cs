using System;
using System.Collections.Generic;
using System.Linq;

namespace SimplexMethod
{
    public class Constraint
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
            if (!Variables.ContainsKey(varName)) Variables.Add(varName, coefficient);
            else Variables[varName] += coefficient;
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

        public static bool TryParse(string constraintString, out Constraint constraint)
        {
            constraint = new Constraint();
            string coefficientText = "";
            bool negitive = false;
            bool postInequality = false;
            bool enteringConstant = false;
            int i = 0;
            foreach (char character in constraintString)
            {
                i++;
                if (postInequality && (enteringConstant && (character == ' ') || i == constraintString.Length))
                {
                    if (i == constraintString.Length) coefficientText += character;
                    if (double.TryParse(coefficientText, out double coefficient))
                        constraint.SetConstant(coefficient * (negitive ? -1 : 1));
                    break;
                }
                else if (character == ' ') continue;
                else if (character == '+') negitive = false;
                else if (character == '-') negitive = true;
                else if (character == '<') postInequality = true;
                else if (postInequality && char.IsDigit(character)) enteringConstant = true;

                if (character == '.' || char.IsDigit(character)) coefficientText += character;
                else if (!postInequality && char.IsLetter(character))
                {
                    if (string.IsNullOrWhiteSpace(coefficientText)) coefficientText = "1";
                    if (double.TryParse(coefficientText, out double coefficient)) // HERE IS THE PROBLEM
                        constraint.AddVariable(character, coefficient * (negitive ? -1 : 1));
                    coefficientText = "";
                }
            }
            return postInequality;
        }
    }
}
