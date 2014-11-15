using RaikesSimplexService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace RaikesSimplexService.Implementation.Extensions
{
    public static class DataModelMatrixify
    {
        public static string Matrixify(this Model model)
        {
            //var rhs = string.Join("\n", model.Constraints.Select(s => s.Value));
            //var lhs = string.Join("\n", model.Constraints.Select(s => string.Join("\t", s.Coefficients)));
            //var objectiveRow = string.Join("\t", model.Goal.Coefficients.Select(s => 0 - s));
            var lhs = Matrix<double>.Build.DenseOfRowArrays(model.Constraints.Select(s => s.Coefficients));
            var rhs = Vector<double>.Build.DenseOfEnumerable(model.Constraints.Select(s => s.Value));
            var objectiveRow = Vector<double>.Build.DenseOfEnumerable(model.Goal.Coefficients.Concat<double>(new double[] { model.Goal.ConstantTerm }));
            return string.Format("LHS:\n{0}\n\nRHS:\n{1}\n\nObjective Row:\n{2}", lhs, rhs, objectiveRow.ToRowMatrix());
        }
    }
}
