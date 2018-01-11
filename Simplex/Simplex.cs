using System;
using System.Collections.Generic;
using System.Linq;

namespace SimplexMethod
{
    public class Simplex
    {
        public LinearProgram LinearProgram { get; set; }

        public Simplex(LinearProgram linearProgram)
        {
            LinearProgram = linearProgram;
        }

        public LPSolution Solve()
        {
            LinearProgram.VerifyVariables();
            
            Tableau previousTableau = new Tableau();

            ConstraintRow[] constraints = new ConstraintRow[LinearProgram.Constraints.Length];
            for (int i = 0; i < constraints.Length; i++)
                constraints[i] = new ConstraintRow(i, constraints.Length, LinearProgram.Constraints[i].GetVars(), LinearProgram.Constraints[i].Constant);

            previousTableau.Constraints = constraints;
            previousTableau.ObjectiveRow = new ObjectiveRow(constraints.Length, LinearProgram.ObjectiveFunction.GetVars());

            Variable[] emptyVars = previousTableau.ObjectiveRow.Vars.Where(var => !var.IsSlack).Select(var => var.Empty).ToArray();
            Tableau currentTableau = Tableau.EmptyTableau(previousTableau.Constraints.Length, emptyVars);
            while (MostNegitive(previousTableau, out Variable mostNegitive))
            {
                int index = 0;
                double leastPositive = previousTableau.Constraints.Length > 0 ? previousTableau.Constraints[index].RatioTest(mostNegitive) : -1;
                for (int i = 0; i < previousTableau.Constraints.Length; i++)
                {
                    double rt = previousTableau.Constraints[i].RatioTest(mostNegitive);
                    if (rt > 0 && rt < leastPositive) { leastPositive = rt; index = i; }
                }
                
                if (leastPositive == 0) throw new Exception("Least Positive is Unchanged");
                
                currentTableau.Constraints[index] = previousTableau.Constraints[index].PivotRow(mostNegitive.Index);
                
                currentTableau.ObjectiveRow = previousTableau.ObjectiveRow.Normalize(mostNegitive.Index, currentTableau.Constraints[index].Vars, currentTableau.Constraints[index].RHS);
                
                for (int i = 0; i < currentTableau.Constraints.Length; i++)
                    if (i != index)
                        currentTableau.Constraints[i] = previousTableau.Constraints[i].Normalize(mostNegitive.Index, currentTableau.Constraints[index].Vars, currentTableau.Constraints[index].RHS);
                
                previousTableau = currentTableau.Copy();
                currentTableau = Tableau.EmptyTableau(previousTableau.Constraints.Length, emptyVars);
            }

            return new LPSolution(previousTableau);
        }

        private bool MostNegitive(Tableau tablar, out Variable mostNegitive)
        {
            mostNegitive = new Variable { Index = -1, Value = 0 };
            foreach (Variable var in tablar.ObjectiveRow.Vars)
                if (var.Value < mostNegitive.Value) mostNegitive = var;
            
            return mostNegitive.Index != -1;
        }
    }

    public class LPSolution
    {
        public Variable[] Results { get; private set; }
        public double Maximum { get; private set; }

        public LPSolution() { }

        public LPSolution(Tableau finalTableau)
        {
            Results = finalTableau.BasicVariableAnalysis();
            Maximum = finalTableau.ObjectiveRow.RHS * 2;
        }

        public void Output()
        {
            Console.WriteLine(string.Join(", ", Results.ToList()));
            Console.WriteLine($"Maximum: { Maximum.ToString("0.00") }");
        }
    }

    public class Tableau
    {
        public ObjectiveRow ObjectiveRow { get; set; }
        public ConstraintRow[] Constraints { get; set; }

        public Tableau() { }

        public static Tableau EmptyTableau(int constraintCount, Variable[] emptyVariables)
        {
            ConstraintRow[] constraints = new ConstraintRow[constraintCount];
            for (int i = 0; i < constraints.Length; i++)
                constraints[i] = new ConstraintRow(i, constraintCount, new List<Variable>(emptyVariables).ToArray(), 0);
            return new Tableau() { Constraints = constraints, ObjectiveRow = new ObjectiveRow(constraints.Length, new List<Variable>(emptyVariables).ToArray()) };
        }

        public void Output()
        {
            for (int i = 0; i < ObjectiveRow.Vars.Length; i++)
                Console.Write($"{ (ObjectiveRow.Vars[i].IsSlack ? $"s<{ i - ObjectiveRow.Vars[i].SlackIndex }>" : ObjectiveRow.Vars[i].Placeholder.ToString()) }\t");
            Console.WriteLine($"RHS");
            for (int i = 0; i < Constraints.Length; i++)
            {
                for (int j = 0; j < Constraints[i].Vars.Length; j++)
                    Console.Write($"{ Constraints[i].Vars[j].Value.ToString("0.00") }\t");
                Console.WriteLine($"{ Constraints[i].RHS.ToString("0.00") }");
            }
            for (int i = 0; i < ObjectiveRow.Vars.Length; i++)
                Console.Write($"{ ObjectiveRow.Vars[i].Value.ToString("0.00") }\t");
            Console.WriteLine($"{ ObjectiveRow.RHS.ToString("0.00") }");
        }

        public Tableau Copy()
        {
            ConstraintRow[] constraintRow = new ConstraintRow[Constraints.Length];
            Constraints.CopyTo(constraintRow, 0);
            return new Tableau() { Constraints = constraintRow, ObjectiveRow = new ObjectiveRow() { Vars = new List<Variable>(ObjectiveRow.Vars).ToArray(), RHS = ObjectiveRow.RHS } };
        }

        public Variable[] BasicVariableAnalysis()
        {
            List<Variable> vars = new List<Variable>();
            for (int i = 0; i < ObjectiveRow.Vars.Length; i++)
            {
                bool hasUnit = false;
                bool isBasic = true;
                for (int j = 0; j < Constraints.Length; j++)
                {
                    double value = Constraints[j].Vars[i].Value;
                    if (value == 1 && !hasUnit) hasUnit = true;
                    else if ((hasUnit && value != 0) || (!hasUnit && value != 0)) isBasic = false;
                }
                
                if (isBasic)
                {
                    for (int j = 0; j < Constraints.Length; j++)
                        if (Constraints[j].Vars[i].Value == 1d)
                            vars.Add(new Variable()
                            {
                                Index = ObjectiveRow.Vars[i].Index,
                                Placeholder = ObjectiveRow.Vars[i].Placeholder,
                                IsSlack = ObjectiveRow.Vars[i].IsSlack,
                                SlackIndex = ObjectiveRow.Vars[i].SlackIndex,
                                Value = Constraints[j].RHS * 2
                            });
                }
                else vars.Add(new Variable()
                {
                    Index = ObjectiveRow.Vars[i].Index,
                    Placeholder = ObjectiveRow.Vars[i].Placeholder,
                    IsSlack = ObjectiveRow.Vars[i].IsSlack,
                    SlackIndex = ObjectiveRow.Vars[i].SlackIndex,
                    Value = 0
                });
            }

            return vars.ToArray();
        }
    }

    public class ObjectiveRow
    {
        public Variable[] Vars { get; set; }
        public double RHS { get; set; }

        public ObjectiveRow() { }

        public ObjectiveRow(int constraintCount, Variable[] vars)
        {
            Vars = new Variable[vars.Length + constraintCount];
            for (int i = 0; i < vars.Length; i++)
                Vars[i] = new Variable() { Index = i, IsSlack = false, Value = vars[i].Value, Placeholder = vars[i].Placeholder, };
            for (int i = vars.Length; i < Vars.Length; i++)
                Vars[i] = new Variable() { Index = i, IsSlack = true, SlackIndex = i - vars.Length, Placeholder = null, Value = 0, };
            RHS = 0;
        }

        public ObjectiveRow Normalize(int pivotIndex, Variable[] pivotVars, double pivotRhs)
        {
            Variable[] newVars = new Variable[Vars.Length];
            for (int i = 0; i < Vars.Length; i++)
                newVars[i] = new Variable() { Index = i, IsSlack = Vars[i].IsSlack, SlackIndex = Vars[i].SlackIndex, Placeholder = Vars[i].Placeholder, Value = (-Vars[pivotIndex].Value * pivotVars[i].Value) + Vars[i].Value };
            return new ObjectiveRow() { Vars = newVars, RHS = (-Vars[pivotIndex].Value * pivotRhs) + RHS };
        }
    }

    public class ConstraintRow
    {
        public int Index { get; set; }
        public Variable[] Vars { get; set; }
        public double RHS { get; set; }

        public ConstraintRow() { }

        public ConstraintRow(int index, int constraintCount, Variable[] vars, double rhs)
        {
            Index = index;
            Vars = new Variable[vars.Length + constraintCount];
            for (int i = 0; i < vars.Length; i++)
                Vars[i] = new Variable() { Index = i, IsSlack = false, SlackIndex = -1, Value = vars[i].Value, Placeholder = vars[i].Placeholder, };
            for (int i = vars.Length; i < Vars.Length; i++)
                Vars[i] = new Variable() { Index = i, IsSlack = true, SlackIndex = i - vars.Length, Value = (i - vars.Length == i ? 1 : 0), };
            RHS = rhs;
        }

        public double RatioTest(Variable var)
        {
            return RHS / Vars[var.Index].Value;
        }

        public ConstraintRow PivotRow(int pivotIndex)
        {
            Variable[] newVars = new Variable[Vars.Length];
            for (int i = 0; i < Vars.Length; i++)
                newVars[i] = new Variable { Index = i, IsSlack = Vars[i].IsSlack, SlackIndex = Vars[i].SlackIndex, Placeholder = Vars[i].Placeholder, Value = Vars[i].Value / Vars[pivotIndex].Value };

            return new ConstraintRow() { Index = Index, Vars = newVars, RHS = RHS / Vars[pivotIndex].Value };
        }

        public ConstraintRow Normalize(int pivotIndex, Variable[] pivotVars, double pivotRhs)
        {
            Variable[] newVars = new Variable[Vars.Length];
            for (int i = 0; i < Vars.Length; i++)
                newVars[i] = new Variable() { Index = i, IsSlack = Vars[i].IsSlack, SlackIndex = Vars[i].SlackIndex, Placeholder = Vars[i].Placeholder, Value = (-Vars[pivotIndex].Value * pivotVars[i].Value) + Vars[i].Value };
            return new ConstraintRow() { Index = Index, Vars = newVars, RHS = (-Vars[pivotIndex].Value * pivotRhs) + RHS };
        }
    }
    
    public class Variable
    {
        public int Index { get; set; }
        public char? Placeholder { get; set; }
        public double Value { get; set; }
        public bool IsSlack { get; set; }
        public int SlackIndex { get; set; }

        public Variable Empty { get { return new Variable() { Index = Index, Placeholder = Placeholder, IsSlack = IsSlack, Value = 0, SlackIndex = -1 }; } }

        public override string ToString()
        {
            return !IsSlack ? $"{ Placeholder } = { Value.ToString("0.00") }" : $"slack<{ SlackIndex }> = { Value.ToString("0.00") }";
        }
    }
}
