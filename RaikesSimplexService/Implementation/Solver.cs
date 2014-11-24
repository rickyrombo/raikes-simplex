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
        private Model _model;
        public Model model
        {
            get { return this._model; }
            private set
            {
                this._model = value;
                this.lhs = value.GetLHSMatrix();
                this.rhs = value.GetRHSVector();
                this.objectiveRow = value.GetObjectiveRow();
            }
        }
        private Matrix<double> lhs;
        private Matrix<double> objectiveRow;
        private Vector<double> rhs;

        public Solution Solve(Model m)
        {
            this.model = Solver.Standardize(m);
            //Get initial basic columns
            var basicColumns = this.lhs.EnumerateColumnsIndexed().Where(v => v.Item2.Count(s => s != 0) == 1 && v.Item2.Any(s => s == 1)).ToList();
            var sol = new Solution();
            var decisionVariableCount = m.Constraints.First().Coefficients.Count();
            while (true){
                //Get the indices of the basic columns relatve to the whole lhs
                var basicColumnIndices = basicColumns.Select(s => s.Item1).ToList();
                //Use those indices to get the nonbasic columns
                var nonbasicColumns = this.lhs.EnumerateColumnsIndexed().Where(v => !basicColumnIndices.Contains(v.Item1)).ToList();
                //Build bInv from the basic columns
                var bInv = Matrix<double>.Build.DenseOfColumnVectors(basicColumns.Select(s => s.Item2)).Inverse();
                //Get the P1' P2' etc from the nonbasic columns * inverse basic matrix
                var primes = nonbasicColumns.Select(
                    s => new Tuple<int, Vector<double>>(s.Item1, bInv.Multiply(s.Item2))
                ).ToList();
                //Get Xb by multiplying the rhs * binv
                var xb = bInv.Multiply(this.rhs);
                //Get the CB from the objective row values corresponding to the basic variables
                var cb = Matrix<double>.Build.DenseOfColumnVectors(
                    this.objectiveRow.EnumerateColumnsIndexed()
                    .Where(s => basicColumnIndices.Contains(s.Item1))
                    .Select(s => s.Item2)
                );
                //Calculate C1' C2' etc and select the minimum - that's our entering basic variable
                var enteringCol = primes.Select(
                    p => (objectiveRow.At(0, p.Item1) - cb.Multiply(p.Item2)).Select(
                        s => new Tuple<int, double>(p.Item1, (double)s)
                    ).First() //There's only ever one element because the width of cb is equal to the height of the primes
                ).OrderBy(s => s.Item2).FirstOrDefault();
                //If all the C1' C2' etc are positive, then we're done - we've optimized the solution
                if (enteringCol.Item2 >= 0) { 
                    sol.Decisions = new double[decisionVariableCount];
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
                //Replace the exiting basic variable with the entering basic variable in basicColumns
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

        public static Model Standardize(Model unstandard)
        {
            var model = new Model
            {
                Constraints = unstandard.Constraints.Select(
                s => new LinearConstraint
                {
                    Coefficients = s.Coefficients.ToArray<double>(),
                    Relationship = s.Relationship,
                    Value = s.Value
                }).ToList(),
                Goal = new Goal { Coefficients = unstandard.Goal.Coefficients.ToArray(), ConstantTerm = unstandard.Goal.ConstantTerm },
                GoalKind = unstandard.GoalKind
            };
            //Minimize => Maximize
            if (model.GoalKind == GoalKind.Minimize)
            {
                model.Goal.Coefficients = model.Goal.Coefficients.Select(s => 0 - s).ToArray<double>();
                model.GoalKind = GoalKind.Maximize;
            }

            var artificialCount = model.Constraints.Count(s => s.Relationship == Relationship.GreaterThanOrEquals || s.Relationship == Relationship.Equals);
            var slackCount = model.Constraints.Count(s => s.Relationship == Relationship.LessThanOrEquals || s.Relationship == Relationship.GreaterThanOrEquals);
            var sVar = 0;
            var aVar = 0;

            foreach (var constraint in model.Constraints)
            {
                var addedVariables = new double[slackCount + artificialCount];
                var coeffs = constraint.Coefficients.ToList<double>();
                if (constraint.Relationship == Relationship.GreaterThanOrEquals)
                {
                    constraint.Relationship = Relationship.Equals;
                    addedVariables[sVar++] = -1;
                    addedVariables[slackCount + aVar++] = 1;
                }
                else if (constraint.Relationship == Relationship.LessThanOrEquals)
                {
                    constraint.Relationship = Relationship.Equals;
                    addedVariables[sVar++] = 1;
                }
                else if (constraint.Relationship == Relationship.Equals)
                {
                    addedVariables[slackCount + aVar++] = 1;
                }
                coeffs.AddRange(addedVariables);
                constraint.Coefficients = coeffs.ToArray<double>();
            }
            model.Goal.Coefficients = new double[model.Goal.Coefficients.Count() + slackCount + artificialCount];
            var i = 0;
            foreach (var coeff in unstandard.Goal.Coefficients)
            {
                model.Goal.Coefficients[i++] = 0 - coeff;
            }
            return model;
        }
    }
}
