using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using RaikesSimplexService.Contracts;
using RaikesSimplexService.DataModel;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using RaikesSimplexService.Implementation.Extensions;

namespace RaikesSimplexService.Implementation
{
    public class Solver : ISolver
    {

        public Solution Solve(Model m)
        {
            var model = StandardModel.FromModel(m);
            return SolveStandardModel(model);
        }

        public Tuple<int, double> CnPrime(StandardModel model, Matrix<double> cb, Tuple<int, Vector<double>> p)
        {
            double cn = model.ObjectiveRow.At(0, p.Item1);
            double cnPrime = cn - cb.Multiply(p.Item2).At(0);
            return new Tuple<int, double>(p.Item1, cnPrime);
        }

        public Tuple<int, double> GetMinCnPrime(IEnumerable<Tuple<int, Vector<double>>> primes, StandardModel model, Matrix<double> cb, List<int> basicColumnIndices)
        {
            var cnPrimes = primes.Where(p => !basicColumnIndices.Contains(p.Item1)).Select(
                    p => CnPrime(model, cb, p)
                );
            Tuple<int, double> minCnPrime = cnPrimes.OrderBy(s => s.Item2).FirstOrDefault(); // Get min
            return minCnPrime;
        }
        public Solution SolveStandardModel(StandardModel model)
        {
            //Get initial basic columns
            var basicColumns = model.LHS.EnumerateColumnsIndexed().Where(v => v.Item2.Count(s => !s.NearlyZero()) == 1 && v.Item2.Any(s => s.NearlyEqual(1))).ToList();
            var sol = new Solution();
            while (true)
            {
                List<int> basicColumnIndices = basicColumns.Select(s => s.Item1).ToList();
                var nonbasicColumns = model.LHS.EnumerateColumnsIndexed().Where(v => !basicColumnIndices.Contains(v.Item1)).ToList();
                var bInv = Matrix<double>.Build.DenseOfColumnVectors(basicColumns.Select(s => s.Item2)).Inverse();
                //Get the P1' P2' etc from the nonbasic columns * inverse basic matrix
                List<Tuple<int, Vector<double>>> primes = model.LHS.EnumerateColumnsIndexed().Select(
                    s => new Tuple<int, Vector<double>>(s.Item1, bInv.Multiply(s.Item2))
                ).ToList();
                var xb = bInv.Multiply(model.RHS);
                var cb = Matrix<double>.Build.DenseOfColumnVectors(
                    model.ObjectiveRow.EnumerateColumnsIndexed()
                    .Where(s => basicColumnIndices.Contains(s.Item1))
                    .Select(s => s.Item2)
                );
                //Calculate C1' C2' etc and select the minimum - that's our entering basic variable
                var minCnPrime = GetMinCnPrime(primes, model, cb, basicColumnIndices);
                //If all the C1' C2' etc are positive, then we're done - we've optimized the solution
                if (minCnPrime.Item2 >= 0 || minCnPrime.Item2.NearlyZero())
                {
                    if (model.ArtificialVariables > 0)
                    {
                        //.Where(pair => pair.Item1 < model.SlackVariables + model.DecisionVariables && pair.Item1 > 0)
                        var phase2 = new StandardModel(model.LHS.RowCount - 1, model.DecisionVariables - 1, model.SlackVariables, 0, model.OriginalModel)
                        {
                            LHS = Matrix.Build.DenseOfColumnVectors(
                                primes.OrderBy(pair => pair.Item1)
                                .Select(p => p.Item2)),
                            RHS = xb,
                            ObjectiveRow = Matrix.Build.DenseOfRowVectors(model.LHS.Row(model.LHS.RowCount - 1)),
                        };
                        var zRowIndex = phase2.LHS.Column(0)
                            .EnumerateIndexed()
                            .Where(pair => pair.Item2.NearlyEqual(1.0))
                            .Select(pair => pair.Item1)
                            .FirstOrDefault();
                        phase2.LHS = phase2.LHS.RemoveRow(zRowIndex);
                        phase2.RHS = Vector<double>.Build.DenseOfEnumerable(
                            phase2.RHS.EnumerateIndexed().Where(pair => pair.Item1 != zRowIndex).Select(pair => pair.Item2)
                        );
                        phase2.LHS = phase2.LHS.RemoveColumn(0);
                        phase2.ObjectiveRow = phase2.ObjectiveRow.RemoveColumn(0);
                        for (int i = 0; i < model.ArtificialVariables; i++)
                        {
                            var artificialCol = phase2.LHS.Column(phase2.LHS.ColumnCount - 1).Enumerate();
                            if (artificialCol.Count(s => !s.NearlyZero()) == 1 && artificialCol.Any(s => s.NearlyEqual(1.0)))
                            {
                                sol.Quality = SolutionQuality.Infeasible;
                                return sol;
                            }
                            phase2.LHS = phase2.LHS.RemoveColumn(phase2.LHS.ColumnCount - 1);
                            phase2.ObjectiveRow = phase2.ObjectiveRow.RemoveColumn(phase2.ObjectiveRow.ColumnCount - 1);
                        }
                        sol = SolveStandardModel(phase2);
                    }
                    else
                    {
                        sol.Decisions = new double[model.DecisionVariables];
                        _mapDecisionVariables(sol.Decisions, basicColumnIndices, xb);
                        sol.OptimalValue = _calculateGoalValue(sol.Decisions, model.OriginalModel.Goal.Coefficients);
                        sol.AlternateSolutionsExist = sol.Decisions.Any(s => s.NearlyZero());
                    }
                    break;
                }
                //else, get the divisor from the Pn' we selected
                var enteringVar = minCnPrime;
                var divisor = primes.Where(s => s.Item1 == enteringVar.Item1).FirstOrDefault().Item2;
                Vector<double> ratios = xb.PointwiseDivide(divisor);
                List<Tuple<int, double>> columnsWithRatios = new List<Tuple<int, double>>();
                for (int i = 0; i < basicColumns.Count; i++)
                {
                    columnsWithRatios.Add(new Tuple<int, double>(basicColumnIndices[i], ratios[i]));
                }
                //Get the minimum ratio that's > 0 - that's our exiting basic variable
                var exitCol = ratios.EnumerateIndexed().Where(s => s.Item2 > 0 && !s.Item2.NearlyZero() && model.ArtificialVariables == 0 || IndexArtificial(basicColumns[s.Item1].Item1, model)).OrderBy(s => s.Item2).FirstOrDefault();
                if (exitCol == null)
                {
                    sol.Quality = SolutionQuality.Unbounded;
                    break;
                }
                var newCol = nonbasicColumns.FirstOrDefault(s => s.Item1 == enteringVar.Item1);
                //basicColumns[exitCol.Item1] = newCol;
                basicColumns.RemoveAt(exitCol.Item1);
                int insertHere = 0;
                foreach (var col in basicColumnIndices)
                {
                    if (col > newCol.Item1)
                    {
                        break;
                    }
                    insertHere++;
                }
                basicColumns.Insert(insertHere, newCol);

            }
            return sol;
        }

        public bool IndexArtificial(int i, StandardModel model)
        {
            return i >= model.DecisionVariables + model.SlackVariables;
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
