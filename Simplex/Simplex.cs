using System;
using System.Collections.Generic;
using System.Linq;

// https://www.youtube.com/watch?v=iwDiG2mR6FM

namespace Simplex
{
    class Simplex
    {
        public LinearProgram LinearProgram { get; set; }

        public Simplex(LinearProgram linearProgram)
        {
            LinearProgram = linearProgram;
        }

        public void Solve()
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
            previousTableau.Output();

            foreach (Variable var in previousTableau.BasicAnalysis())
                Console.WriteLine($"{ var.Placeholder } = { var.Value }");
            Console.WriteLine($"Maximum: { previousTableau.ObjectiveRow.RHS }");
        }

        private bool MostNegitive(Tableau tablar, out Variable mostNegitive)
        {
            mostNegitive = new Variable { Index = -1, Value = 0 };
            foreach (Variable var in tablar.ObjectiveRow.Vars)
                if (var.Value < mostNegitive.Value) mostNegitive = var;
            
            return mostNegitive.Index != -1;
        }
    }

    class Tableau
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
            for (int j = 0; j < ObjectiveRow.Vars.Length; j++)
                Console.Write($"{ ObjectiveRow.Vars[j].Value.ToString("0.00") } ");
            Console.WriteLine($"{ ObjectiveRow.RHS.ToString("0.00") }");
            for (int i = 0; i < Constraints.Length; i++)
            {
                for (int j = 0; j < Constraints[i].Vars.Length; j++)
                {
                    Console.Write($"{ Constraints[i].Vars[j].Value.ToString("0.00") } ");
                }
                Console.WriteLine($"{ Constraints[i].RHS.ToString("0.00") }");
            }
        }

        public Tableau Copy()
        {
            ConstraintRow[] constraintRow = new ConstraintRow[Constraints.Length];
            Constraints.CopyTo(constraintRow, 0);
            return new Tableau() { Constraints = constraintRow, ObjectiveRow = new ObjectiveRow() { Vars = new List<Variable>(ObjectiveRow.Vars).ToArray(), RHS = ObjectiveRow.RHS } };
        }

        public Variable[] BasicAnalysis()
        {


            return new Variable[0];
        }
    }

    class ObjectiveRow
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
                Vars[i] = new Variable() { Index = i, IsSlack = true, Value = 0, };
            RHS = 0;
        }

        public ObjectiveRow Normalize(int pivotIndex, Variable[] pivotVars, double pivotRhs)
        {
            Variable[] newVars = new Variable[Vars.Length];
            for (int i = 0; i < Vars.Length; i++)
                newVars[i] = new Variable() { Index = i, IsSlack = Vars[i].IsSlack, Placeholder = Vars[i].Placeholder, Value = (-Vars[pivotIndex].Value * pivotVars[i].Value) + Vars[i].Value };
            return new ObjectiveRow() { Vars = newVars, RHS = (-Vars[pivotIndex].Value * pivotRhs) + RHS };
        }
    }

    class ConstraintRow
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
                Vars[i] = new Variable() { Index = i, IsSlack = false, Value = vars[i].Value, Placeholder = vars[i].Placeholder, };
            for (int i = vars.Length; i < Vars.Length; i++)
                Vars[i] = new Variable() { Index = i, IsSlack = true, Value = (i - vars.Length == i ? 1 : 0), };
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
                newVars[i] = new Variable { Index = i, IsSlack = Vars[i].IsSlack, Placeholder = Vars[i].Placeholder, Value = Vars[i].Value / Vars[pivotIndex].Value };

            return new ConstraintRow() { Index = Index, Vars = newVars, RHS = RHS / Vars[pivotIndex].Value };
        }

        public ConstraintRow Normalize(int pivotIndex, Variable[] pivotVars, double pivotRhs)
        {
            Variable[] newVars = new Variable[Vars.Length];
            for (int i = 0; i < Vars.Length; i++)
                newVars[i] = new Variable() { Index = i, IsSlack = Vars[i].IsSlack, Placeholder = Vars[i].Placeholder, Value = (-Vars[pivotIndex].Value * pivotVars[i].Value) + Vars[i].Value };
            return new ConstraintRow() { Index = Index, Vars = newVars, RHS = (-Vars[pivotIndex].Value * pivotRhs) + RHS };
        }
    }
    
    class Variable
    {
        public int Index { get; set; }
        public char? Placeholder { get; set; }
        public double Value { get; set; }
        public bool IsSlack { get; set; }

        public Variable Empty { get { return new Variable() { Index = Index, Placeholder = Placeholder, IsSlack = IsSlack, Value = 0 }; } }
    }
}
