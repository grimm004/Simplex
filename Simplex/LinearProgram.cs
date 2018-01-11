using System.Linq;

namespace SimplexMethod
{
    public class LinearProgram
    {
        public ObjectiveFunction ObjectiveFunction { get; set; }
        public Constraint[] Constraints { get; set; }

        public LinearProgram()
        {
            ObjectiveFunction = new ObjectiveFunction();
            Constraints = new Constraint[0];
        }

        public void VerifyVariables()
        {
            for (int i = 0; i < Constraints.Length ; i++)
                Constraints[i].VerifyVariables(ObjectiveFunction.Variables.Keys.ToArray());
        }

        public void Output()
        {
            System.Console.Write("Maximize \t");
            ObjectiveFunction.Output();
            System.Console.Write("Subject to \t");
            if (Constraints.Length > 0)
            {
                Constraints[0].Output();
                for (int i = 1; i < Constraints.Length; i++) { System.Console.Write("\t\t"); Constraints[i].Output(); }
            }
        }
    }
}
