using RaikesSimplexService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaikesSimplexService.Implementation.Extensions
{
    public static class DataModelMatrixify
    {
        public static string Matrixify(this Model model)
        {
            var rhs = string.Join("\n", model.Constraints.Select(s => s.Value));
            var lhs = string.Join("\n", model.Constraints.Select(s => string.Join("\t", s.Coefficients)));
            var objectiveRow = string.Join("\t", model.Goal.Coefficients.Select(s => 0 - s));
            return string.Format("LHS:\n{0}\n\nRHS:\n{1}\n\nObjective Row:\n{2}", lhs, rhs, objectiveRow);
        }
    }
}
