using System.Collections.Generic;

namespace SimplexMethod
{
    public class LPParser
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

        public bool SetObjectiveFunction(string objectiveFunctionString)
        {
            if (ObjectiveFunction.TryParse(objectiveFunctionString, out ObjectiveFunction objectiveFunction))
                ObjectiveFunction = objectiveFunction;
            else return false;
            return true;
        }

        public bool AddConstraint(string constraintString)
        {
            if (Constraint.TryParse(constraintString, out Constraint constraint))
                Constraints.Add(constraint);
            else return false;
            return true;
        }

        public void ClearConstraints()
        {
            Constraints.Clear();
        }
    }
}
