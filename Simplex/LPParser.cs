using System;
using System.Collections.Generic;
using System.Text;

namespace Simplex
{
    class LPParser
    {
        public LinearProgram LinearProgram
        {
            get
            {
                return new LinearProgram() { ObjectiveFunction = ObjectiveFunction, Constraints = Constraints.ToArray() };
            }
        }

        private ObjectiveFunction ObjectiveFunction { get; set; }
        private List<Constraint> Constraints { get; set; }

        public LPParser()
        {
            ObjectiveFunction = new ObjectiveFunction();
            Constraints = new List<Constraint>();
        }

        public void SetObjectiveFunction(string objectiveFunctionString)
        {
            ObjectiveFunction = new ObjectiveFunction();
            
            string coefficientText = "";
            bool negitive = false;
            foreach (char character in objectiveFunctionString)
            {
                if (character == ' ') continue;
                else if (character == '+') negitive = false;
                else if (character == '-') negitive = true;
                
                if (character == '.' || char.IsDigit(character)) coefficientText += character;
                else if (char.IsLetter(character))
                {
                    if (string.IsNullOrWhiteSpace(coefficientText)) coefficientText = "1";
                    if (double.TryParse(coefficientText, out double coefficient))
                        ObjectiveFunction.AddVariable(character, coefficient * (negitive ? -1 : 1));
                    coefficientText = "";
                }
            }
        }

        public void AddConstraint(string constraintString)
        {
            Constraint constraint = new Constraint();

            string coefficientText = "";
            bool negitive = false;
            bool postInequality = false;
            bool enteringConstant = false;
            int i = 0;
            foreach (char character in constraintString)
            {
                i++;
                if (postInequality && enteringConstant && (character == ' ' || i == constraintString.Length))
                {
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
                    if (double.TryParse(coefficientText, out double coefficient))
                        constraint.AddVariable(character, coefficient * (negitive ? -1 : 1));
                    coefficientText = "";
                }
            }

            Constraints.Add(constraint);
        }
    }
}
