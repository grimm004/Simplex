using System;
using System.Collections.Generic;
using System.Text;
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

            List<Tablar> tablars = new List<Tablar>();

            Tablar firstTablar = new Tablar();

            ConstraintRow[] constraints = new ConstraintRow[LinearProgram.Constraints.Length];
            for (int i = 0; i < constraints.Length; i++)
                constraints[i] = new ConstraintRow(i, constraints.Length, LinearProgram.Constraints[i].GetVars(), LinearProgram.Constraints[i].Constant);

            firstTablar.Constraints = constraints;
            firstTablar.ObjectiveRow = new ObjectiveRow(constraints.Length, LinearProgram.ObjectiveFunction.GetVars(), 0);

            
            Tablar currentTablar = firstTablar;
            while (MostNegitive(currentTablar, out Var mostNegitive))
            {
                int index = 0;
                double smallestPositive = currentTablar.Constraints.Length > 0 ? currentTablar.Constraints[0].RatioTest(mostNegitive) : 0;
                for (int i = 0; i < currentTablar.Constraints.Length; i++)
                {
                    double rt = currentTablar.Constraints[i].RatioTest(mostNegitive);
                    if (rt > 0 && rt < smallestPositive) { smallestPositive = rt; index = i; }
                }

                currentTablar.Constraints[index] = (1 / smallestPositive) * currentTablar.Constraints[index];

                currentTablar.ObjectiveRow = (currentTablar.Constraints[index] - (currentTablar.ObjectiveRow.Vars[mostNegitive.Index].Value * currentTablar.ObjectiveRow));

                currentTablar.ObjectiveRow.RHS = (currentTablar.Constraints[index].RHS - (currentTablar.ObjectiveRow.Vars[mostNegitive.Index].Value * currentTablar.ObjectiveRow.RHS));

                for (int i = 0; i < currentTablar.Constraints.Length; i++)
                    if (i != index)
                    {
                        currentTablar.Constraints[i] = (currentTablar.Constraints[index] - (currentTablar.Constraints[i].Vars[mostNegitive.Index].Value * currentTablar.Constraints[i]));
                        currentTablar.Constraints[i].RHS = (currentTablar.Constraints[index].RHS - (currentTablar.Constraints[i].Vars[mostNegitive.Index].Value * currentTablar.Constraints[i].RHS));
                    }

                Console.WriteLine(currentTablar.ObjectiveRow.RHS);
            }
        }

        private bool MostNegitive(Tablar tablar, out Var mostNegitive)
        {
            mostNegitive = new Var { Index = -1, Value = 0 };
            foreach (Var var in tablar.ObjectiveRow.Vars)
                if (var.Value < mostNegitive.Value) mostNegitive = var;

            if (mostNegitive.Index == -1) return false;
            else return true;
        }
    }

    class Tablar
    {
        public ObjectiveRow ObjectiveRow { get; set; }
        public ConstraintRow[] Constraints { get; set; }
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

        public static ObjectiveRow operator *(double left, ObjectiveRow right)
        {
            for (int i = 0; i < right.Vars.Length; i++)
                right.Vars[i].Value *= left;
            return right;
        }

        public static ObjectiveRow operator +(ObjectiveRow left, ConstraintRow right)
        {
            for (int i = 0; i < left.Vars.Length; i++)
                left.Vars[i].Value += right.Vars[i].Value;
            return left;
        }

        public static ObjectiveRow operator -(ObjectiveRow left, ConstraintRow right)
        {
            for (int i = 0; i < left.Vars.Length; i++)
                left.Vars[i].Value -= right.Vars[i].Value;
            return left;
        }

        public static ObjectiveRow operator -(ConstraintRow left, ObjectiveRow right)
        {
            for (int i = 0; i < left.Vars.Length; i++)
                left.Vars[i].Value -= right.Vars[i].Value;
            for (int i = 0; i < left.Vars.Length; i++)
                right.Vars[i].Value = left.Vars[i].Value;
            return right;
        }
    }

    class ConstraintRow
    {
        public Var[] Vars { get; set; }
        public double RHS { get; set; }

        public ConstraintRow(int constraintIndex, int constraintCount, Var[] variables, double rhs)
        {
            RHS = rhs;
            Vars = new Var[variables.Length + constraintCount];
            Array.Copy(variables, 0, Vars, 0, variables.Length);
            for (int i = variables.Length; i < Vars.Length; i++)
                Vars[i] = new Slack() { Index = i, Value = constraintIndex == i ? 1 : 0 };
        }

        public double RatioTest(Var var)
        {
            return RHS / var.Value;
        }

        public static ConstraintRow operator *(double left, ConstraintRow right)
        {
            for (int i = 0; i < right.Vars.Length; i++)
                right.Vars[i].Value *= left;
            return right;
        }

        public static ConstraintRow operator +(ConstraintRow left, ConstraintRow right)
        {
            for (int i = 0; i < left.Vars.Length; i++)
                left.Vars[i].Value += right.Vars[i].Value;
            return left;
        }

        public static ConstraintRow operator -(ConstraintRow left, ConstraintRow right)
        {
            for (int i = 0; i < left.Vars.Length; i++)
                left.Vars[i].Value -= right.Vars[i].Value;
            return left;
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
    }
}
