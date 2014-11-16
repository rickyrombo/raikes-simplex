using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using RaikesSimplexService.Contracts;
using RaikesSimplexService.DataModel;
using MathNet.Numerics.LinearAlgebra;
using RaikesSimplexService.Implementation.Extensions;

namespace RaikesSimplexService.Implementation
{
    public class Solver : ISolver
    {
        public Solution Solve(Model m)
        {
            var model = StandardModel.FromModel(m);
            //Get initial basic columns
            var basicColumns = model.LHS.EnumerateColumnsIndexed().Where(v => v.Item2.Count(s => s != 0) == 1 && v.Item2.Any(s => s == 1)).ToList();
            var sol = new Solution();
            while (true){
                var basicColumnIndices = basicColumns.Select(s => s.Item1).ToList();
                var nonbasicColumns = model.LHS.EnumerateColumnsIndexed().Where(v => !basicColumnIndices.Contains(v.Item1)).ToList();
                var bInv = Matrix<double>.Build.DenseOfColumnVectors(basicColumns.Select(s => s.Item2)).Inverse();
                //Get the P1' P2' etc from the nonbasic columns * inverse basic matrix
                var primes = nonbasicColumns.Select(
                    s => new Tuple<int, Vector<double>>(s.Item1, bInv.Multiply(s.Item2))
                ).ToList();
                var xb = bInv.Multiply(model.RHS);
                var cb = Matrix<double>.Build.DenseOfColumnVectors(
                    model.ObjectiveRow.EnumerateColumnsIndexed()
                    .Where(s => basicColumnIndices.Contains(s.Item1))
                    .Select(s => s.Item2)
                );
                //Calculate C1' C2' etc and select the minimum - that's our entering basic variable
                var enteringCol = primes.Select(
                    p => (model.ObjectiveRow.At(0, p.Item1) - cb.Multiply(p.Item2)).Select(
                        s => new Tuple<int, double>(p.Item1, (double)s)
                    ).First() //There's only ever one element because the width of cb is equal to the height of the primes
                ).OrderBy(s => s.Item2).FirstOrDefault();
                //If all the C1' C2' etc are positive, then we're done - we've optimized the solution
                if (enteringCol.Item2 >= 0) { 
                    sol.Decisions = new double[model.DecisionVariables];
                    _mapDecisionVariables(sol.Decisions, basicColumnIndices, xb);
                    sol.OptimalValue = _calculateGoalValue(sol.Decisions, m.Goal.Coefficients);
                    sol.AlternateSolutionsExist = sol.Decisions.Any(s => s == 0.0);
                    break;
                }
                //else, get the divisor from the Pn' we selected
                var divisor = primes.Where(s => s.Item1 == enteringCol.Item1).FirstOrDefault().Item2;
                var ratios = xb.PointwiseDivide(divisor);
                //Get the minimum ratio that's > 0 - that's our exiting basic variable
                var exitingCol = ratios.EnumerateIndexed().Where(s => s.Item2 > 0).OrderBy(s => s.Item2).FirstOrDefault();
                var newCol = nonbasicColumns.FirstOrDefault(s => s.Item1 == enteringCol.Item1);
                basicColumns[exitingCol.Item1] = newCol;
            }
            return sol;
        }

        /// <summary>
        /// Runs the goal equation on the optimized decision variables
        /// </summary>
        /// <param name="decisionVariables">the array of optimized decision variables</param>
        /// <param name="goalCoeffs">the array of goal coefficients</param>
        /// <returns>The sum of each goal coefficient * the value of the corresponding decision variable</returns>
        private static double _calculateGoalValue(double[] decisionVariables, double[] goalCoeffs)
        {
            var i = 0;
            var sum = 0.0;
            foreach (var decision in decisionVariables)
            {
                sum += goalCoeffs[i++] * decision;
            }
            return sum;
        }

        /// <summary>
        /// Maps decision variables from the final Xb based on the indicies of the basic matrix to an array
        /// </summary>
        /// <param name="decisionVariables">the array of decision variables to be mapped to</param>
        /// <param name="basicColumnIndices">the indicies of the basic variables</param>
        /// <param name="finalVariableValues">the final values in xb</param>
        private static void _mapDecisionVariables(double[] decisionVariables, List<int> basicColumnIndices, Vector<double> finalVariableValues)
        {
            for (var i = 0; i < basicColumnIndices.Count(); i++)
            {
                if (basicColumnIndices[i] < decisionVariables.Length) //ignore slack/artificial variables
                {
                    decisionVariables[basicColumnIndices[i]] = finalVariableValues.At(i);
                }
            }
        }
    }
}
