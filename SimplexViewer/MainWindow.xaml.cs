using SimplexMethod;
using System.Linq;
using System.Windows;

namespace SimplexViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SubmitButtonClick(object sender, RoutedEventArgs e)
        {
            LPParser parser = new LPParser();
            if (parser.SetObjectiveFunction(ObjectiveFunctionEntry.Text))
            {
                bool constraintsValid = true;
                foreach (string constraint in ConstraintEntry.Text.Split('\n'))
                    constraintsValid &= parser.AddConstraint(constraint);

                if (constraintsValid)
                {
                    LPSolution solution = new Simplex(parser.LinearProgram).Solve();
                    ResultsLabel.Content = $"Final Tableau:\n{ solution.FinalTableau.ToString() }\n\nResults:\n{ string.Join(", ", solution.Results.ToList()) }\nMaximum = { solution.Maximum }";
                }
                else ResultsLabel.Content = "Syntax error in constraint.";
            }
            else ResultsLabel.Content = "Syntax error in objective function.";
        }
    }
}
