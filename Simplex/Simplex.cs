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

            List<Tableau> tablars = new List<Tableau>();

            Tableau initialTableau = new Tableau();

            ConstraintRow[] constraints = new ConstraintRow[LinearProgram.Constraints.Length];
            for (int i = 0; i < constraints.Length; i++)
                constraints[i] = new ConstraintRow(i, constraints.Length, LinearProgram.Constraints[i].GetVars(), LinearProgram.Constraints[i].Constant);

            initialTableau.Constraints = constraints;
            initialTableau.ObjectiveRow = new ObjectiveRow(constraints.Length, LinearProgram.ObjectiveFunction.GetVars(), 0);

            initialTableau.Output();
            Console.WriteLine();

            Var[] emptyVars = initialTableau.ObjectiveRow.Vars.Where(var => var.GetType() == typeof(Variable)).Select(var => ((Variable)var).Empty).ToArray();

            Tableau currentTableau = new Tableau(initialTableau.Constraints.Length, initialTableau.ObjectiveRow.Vars.Length, emptyVars), previousTableau = initialTableau.Copy();
            while (MostNegitive(previousTableau, out Var mostNegitive))
            {
                int index = 0;
                double leastPositive = previousTableau.Constraints.Length > 0 ? previousTableau.Constraints[index].RatioTest(mostNegitive) : -1;
                for (int i = 0; i < previousTableau.Constraints.Length; i++)
                {
                    double rt = previousTableau.Constraints[i].RatioTest(mostNegitive);
                    if (rt > 0 && rt < leastPositive) { leastPositive = rt; index = i; }
                }
                
                if (leastPositive == 0) throw new Exception("Least Positive is Unchanged");

                currentTableau.Constraints[index] = (1 / leastPositive) * previousTableau.Constraints[index];

                currentTableau.ObjectiveRow = (previousTableau.Constraints[index] - (previousTableau.ObjectiveRow.Vars[mostNegitive.Index].Value * previousTableau.ObjectiveRow));
                currentTableau.ObjectiveRow.RHS = (previousTableau.Constraints[index].RHS - (previousTableau.ObjectiveRow.Vars[mostNegitive.Index].Value * previousTableau.ObjectiveRow.RHS));
                
                for (int i = 0; i < currentTableau.Constraints.Length; i++)
                    if (i != index)
                    {
                        currentTableau.Constraints[i] = (previousTableau.Constraints[index] - (previousTableau.Constraints[i].Vars[mostNegitive.Index].Value * previousTableau.Constraints[i]));
                        currentTableau.Constraints[i].RHS = (previousTableau.Constraints[index].RHS - (previousTableau.Constraints[i].Vars[mostNegitive.Index].Value * previousTableau.Constraints[i].RHS));
                    }

                previousTableau.Output();

                previousTableau = currentTableau.Copy();
                currentTableau = previousTableau.CopyAttributes();
            }
            Console.WriteLine(previousTableau.ObjectiveRow.RHS);
        }

        private bool MostNegitive(Tableau tablar, out Var mostNegitive)
        {
            mostNegitive = new Var { Index = -1, Value = 0 };
            foreach (Var var in tablar.ObjectiveRow.Vars)
                if (var.Value < mostNegitive.Value) mostNegitive = var;

            Console.WriteLine($"MN: {mostNegitive.Value}");

            return mostNegitive.Index != -1;
        }
    }

    class Tableau
    {
        public ObjectiveRow ObjectiveRow { get; set; }
        public ConstraintRow[] Constraints { get; set; }

        public Tableau() { }
        public Tableau(int constraintCount, int varCount, Var[] emptyVars)
        {
            Constraints = new ConstraintRow[constraintCount];
            for (int i = 0; i < Constraints.Length; i++)
                Constraints[i] = new ConstraintRow(i, constraintCount, new List<Var>(emptyVars).ToArray(), 0);
            ObjectiveRow = new ObjectiveRow(constraintCount, new List<Var>(emptyVars).ToArray(), 0);
        }

        public Tableau Copy()
        {
            return new Tableau()
            {
                ObjectiveRow = ObjectiveRow.Copy(),
                Constraints = new List<ConstraintRow>(Constraints).ToArray()
            };
        }

        public Tableau CopyAttributes()
        {
            Var[] vars = new List<Var>(ObjectiveRow.Vars).ToArray();
            for (int i = 0; i < vars.Length; i++)
                vars[i].Value = 0;

            ConstraintRow[] constraintRows = new ConstraintRow[Constraints.Length];
            for (int i = 0; i < constraintRows.Length; i++)
                constraintRows[i] = new ConstraintRow(i, new List<Var>(vars).ToArray());

            return new Tableau()
            {
                ObjectiveRow = new ObjectiveRow(Constraints.Length, new List<Var>(vars).ToArray(), 0),
                Constraints = constraintRows,
            };
        }

        public void Output()
        {
            for (int j = 0; j < ObjectiveRow.Vars.Length; j++)
            {
                Console.Write($"{ ObjectiveRow.Vars[j].Value.ToString("0.00") } ");
            }
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
    }

    class ObjectiveRow
    {
        public double RHS { get; set; }
        public Var[] Vars { get; set; }

        public ObjectiveRow(int constraintCount, Var[] variables, double rhs)
        {
            RHS = rhs;
            Vars = new Var[variables.Length + constraintCount];
            Array.Copy(variables, 0, Vars, 0, variables.Length);
            for (int i = variables.Length; i < Vars.Length; i++)
                Vars[i] = new Slack() { Index = i, Value = 0 };
        }

        public ObjectiveRow(Var[] vars)
        {
            RHS = 0;
            Vars = vars;
        }

        public ObjectiveRow() { }

        public static ObjectiveRow operator *(double left, ObjectiveRow right)
        {
            for (int i = 0; i < right.Vars.Length; i++)
                right.Vars[i].Value *= left;
            return right;
        }

        public static ObjectiveRow operator +(ObjectiveRow left, ConstraintRow right)
        {
            ObjectiveRow result = left.Copy();
            for (int i = 0; i < result.Vars.Length; i++)
                result.Vars[i].Value += right.Vars[i].Value;
            return result;
        }

        public static ObjectiveRow operator -(ObjectiveRow left, ConstraintRow right)
        {
            ObjectiveRow result = left.Copy();
            for (int i = 0; i < result.Vars.Length; i++)
                result.Vars[i].Value -= right.Vars[i].Value;
            return result;
        }

        public static ObjectiveRow operator -(ConstraintRow left, ObjectiveRow right)
        {
            ConstraintRow resultL = left.Copy();
            ObjectiveRow resultR = right.Copy();
            for (int i = 0; i < resultL.Vars.Length; i++)
                resultL.Vars[i].Value -= resultR.Vars[i].Value;
            for (int i = 0; i < resultL.Vars.Length; i++)
                resultR.Vars[i].Value = resultL.Vars[i].Value;
            return resultR;
        }

        public ObjectiveRow Copy()
        {
            return new ObjectiveRow()
            {
                RHS = RHS,
                Vars = new List<Var>(Vars).ToArray()
            };
        }
    }

    class ConstraintRow
    {
        public int Index { get; set; }
        public Var[] Vars { get; set; }
        public double RHS { get; set; }

        public ConstraintRow(int constraintIndex, int constraintCount, Var[] variables, double rhs)
        {
            Index = constraintIndex;
            RHS = rhs;
            Vars = new Var[variables.Length + constraintCount];
            Array.Copy(variables, 0, Vars, 0, variables.Length);
            for (int i = variables.Length; i < Vars.Length; i++)
                Vars[i] = new Slack() { Index = i, Value = (constraintIndex == (i - variables.Length) ? 1d : 0d) };
        }

        public ConstraintRow(int constraintIndex, Var[] vars)
        {
            Index = constraintIndex;
            RHS = 0;
            Vars = vars;
        }

        public ConstraintRow() { }

        public double RatioTest(Var var)
        {
            return RHS / Vars[var.Index].Value;
        }

        public static ConstraintRow operator *(double left, ConstraintRow right)
        {
            ConstraintRow result = right.Copy();
            for (int i = 0; i < result.Vars.Length; i++)
                result.Vars[i].Value *= left;
            return result;
        }

        public static ConstraintRow operator +(ConstraintRow left, ConstraintRow right)
        {
            ConstraintRow result = left.Copy();
            for (int i = 0; i < result.Vars.Length; i++)
                result.Vars[i].Value += right.Vars[i].Value;
            return result;
        }

        public static ConstraintRow operator -(ConstraintRow left, ConstraintRow right)
        {
            ConstraintRow result = left.Copy();
            for (int i = 0; i < result.Vars.Length; i++)
                result.Vars[i].Value -= right.Vars[i].Value;
            return result;
        }

        public ConstraintRow Copy()
        {
            return new ConstraintRow()
            {
                Index = Index,
                RHS = RHS,
                Vars = new List<Var>(Vars).ToArray()
            };
        }
    }

    class Var
    {
        public int Index { get; set; }
        public double Value { get; set; }
    }

    class Slack : Var { }

    class Variable : Var
    {
        public char Placeholder { get; set; }
        public Variable Empty { get { return new Variable() { Index = Index, Placeholder = Placeholder, Value = 0 }; } }
    }
}
