using MathNet.Numerics.LinearAlgebra;
using RaikesSimplexService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaikesSimplexService.Implementation.Extensions;

namespace RaikesSimplexService.Implementation
{
    public class StandardModel
    {
        #region datamembers
        public enum OutputFormat
        {
            Matrix,
            Original,
            Expression
        }
        public Model OriginalModel { get; private set; }
        public Matrix<double> LHS { get; set; }
        public Vector<double> RHS { get; set; }
        public Matrix<double> ObjectiveRow { get; set; }

        public int DecisionVariables { get; private set; }
        public int SlackVariables { get; private set; }
        public int ArtificialVariables { get; private set; }
        #endregion

        public StandardModel(int constraintCount, int decisionCount, int slackCount, int artificialCount, Model originalModel)
        {
            var totalVars = decisionCount + slackCount + artificialCount;
            this.LHS = Matrix<double>.Build.Dense(constraintCount, totalVars);
            this.RHS = Vector<double>.Build.Dense(constraintCount);
            this.ObjectiveRow = Matrix<double>.Build.Dense(1, totalVars);
            this.DecisionVariables = decisionCount;
            this.SlackVariables = slackCount;
            this.ArtificialVariables = artificialCount;
            this.OriginalModel = originalModel;
        }

        /// <summary>
        /// Builds a new StandardModel in Standard Form from an existing model
        /// </summary>
        /// <param name="model">the model to be standardized</param>
        /// <returns>a new StandardModel in standard form</returns>
        public static StandardModel FromModel(Model model)
        {
            var artificialCount = model.Constraints.Count(s => s.Relationship == Relationship.GreaterThanOrEquals || s.Relationship == Relationship.Equals);
            var slackCount = model.Constraints.Count(s => s.Relationship == Relationship.LessThanOrEquals || s.Relationship == Relationship.GreaterThanOrEquals);
            var wombo = ((artificialCount > 0) ? 1 : 0);

            var standardModel = new StandardModel(
                model.Constraints.Count() + wombo,
                model.Constraints.First().Coefficients.Length + wombo,
                slackCount,
                artificialCount,
                model
            );

            var sVar = 0; //Keeps track of where to put the next slack variable
            var aVar = 0; //Keeps track of where to put the next artificial variable
            var i = 0;

            foreach (var constraint in model.Constraints)
            {
                var addedVariables = new double[slackCount + artificialCount];
                var coeffs = constraint.Coefficients.ToList<double>();
                if (constraint.Relationship == Relationship.GreaterThanOrEquals)
                {
                    addedVariables[sVar++] = -1;
                    addedVariables[slackCount + aVar++] = 1;
                }
                else if (constraint.Relationship == Relationship.LessThanOrEquals)
                {
                    addedVariables[sVar++] = 1;
                }
                else if (constraint.Relationship == Relationship.Equals)
                {
                    addedVariables[slackCount + aVar++] = 1;
                }
                coeffs.AddRange(addedVariables);
                if (artificialCount > 0)
                {
                    //Insert a zero before the rest of the rest of the constraints for "Z" in two phase
                    coeffs.Insert(0, 0);
                }
                standardModel.LHS.SetRow(i++, coeffs.ToArray<double>());
            }
            //Catch for artificial column (z)
            var goalCoeffsList = (artificialCount == 0) ? new List<double>() : new List<double>{0};
            goalCoeffsList.AddRange(model.Goal.Coefficients);
            var goalCoeffs = goalCoeffsList.ToArray<double>();
            Array.Resize<double>(ref goalCoeffs, goalCoeffs.Length + slackCount + artificialCount);
            if (model.GoalKind == GoalKind.Maximize)
            {
                standardModel.ObjectiveRow.SetRow(0, goalCoeffs.Select(s => 0 - s).ToArray<double>());
            }
            else
            {
                standardModel.ObjectiveRow.SetRow(0, goalCoeffs);
            }

            var constraintRhs = model.Constraints.Select(c => c.Value).ToList<double>();
            if (artificialCount > 0)
            {
                constraintRhs.Add(0);
            }
            standardModel.RHS.SetValues(constraintRhs.ToArray<double>());
            if (standardModel.ArtificialVariables > 0)
            {
                standardModel.AddWRow();
            }
            return standardModel;
        }

        private void AddWRow()
        {
            //move objective row to a constraint
            this.LHS.SetRow(this.LHS.RowCount - 1, this.ObjectiveRow.Row(0).ToArray<double>());
            this.LHS.At(this.LHS.RowCount - 1, 0, 1);
            this.ObjectiveRow.SetRow(0, new double[this.LHS.ColumnCount]);
            //Sum all variables for the w row
            foreach(var row in this.LHS.EnumerateRows().Where(r => ContainsArtificalVariable(r)))
            {
                foreach (var pair in row.EnumerateIndexed())
                {
                    this.ObjectiveRow.At(0, pair.Item1, this.ObjectiveRow.At(0, pair.Item1) - pair.Item2);
                }
            }
            //Set artifical values in the objective row to zero
            for (var c = DecisionVariables + SlackVariables; c < this.ObjectiveRow.ColumnCount; c++)
            {
                this.ObjectiveRow.At(0, c, 0);
            }
        }

        private bool ContainsArtificalVariable(Vector<double> r)
        {
            return r.EnumerateIndexed().Any(
                t => t.Item1 >= this.DecisionVariables + this.SlackVariables && t.Item2 == 1
            );
        }

        /// <summary>
        /// Returns the string representation of the model
        /// </summary>
        /// <returns>the model as a string with MathNet's string representation of each matrix</returns>
        public override string ToString()
        {
            return string.Format("LHS:\n{0}\n\nRHS:\n{1}\n\nObjective Row:\n{2}", this.LHS, this.RHS, this.ObjectiveRow);
        }

        /// <summary>
        /// Returns a string representation of the model in the specified format
        /// Matrix is a matrix representation.
        /// Original is without artificial/slack variables. 
        /// Expression is the common mathematicla form.
        /// </summary>
        /// <param name="format">The format of the string</param>
        /// <returns>the model as a string in the specified format</returns>
        public string ToString(OutputFormat format)
        {
            if (format == OutputFormat.Expression)
            {
                return string.Format("Constraints:\n{0}\nObjective: Maximize\nZ\t+ {1} \t= 0",
                    string.Join("\n", LHS.EnumerateRowsIndexed().Select(
                        r => string.Format("{0}\t= {1}", _stringExpression(r.Item2), this.RHS.At(r.Item1))
                    )),
                    _stringExpression(ObjectiveRow.Row(0))
                );
            }
            if (format == OutputFormat.Original)
            {
                return string.Format("Constraints:\n{0}\nObjective: {2}\n{1} \t= Z",
                    string.Join("\n", OriginalModel.Constraints.Select(
                        r => string.Format("{0} \t{1} {2}",
                                _stringExpression(r.Coefficients, true),
                                _getRelationshipSymbol(r.Relationship),
                                r.Value
                            )
                    )),
                    _stringExpression(ObjectiveRow.Row(0), true),
                    OriginalModel.GoalKind
                );
            }
            return _getMatrixForm();
        }

        public bool Equals(StandardModel other)
        {
            bool sameObjRow = this.ObjectiveRow.Equals(other.ObjectiveRow);
            bool sameLHS = this.LHS.Equals(other.LHS);
            bool sameRHS = this.RHS.Equals(other.RHS);
            bool sameSlack = this.SlackVariables == other.SlackVariables;
            bool sameModel = this.OriginalModel.EqualValues(other.OriginalModel);
            return sameObjRow && sameLHS && sameRHS && sameSlack && sameModel;
        }

        #region private methods
        private string _getRelationshipSymbol(Relationship r)
        {
            switch (r)
            {
                case Relationship.LessThanOrEquals:
                    return "<=";
                case Relationship.GreaterThanOrEquals:
                    return ">=";
                default:
                    return "=";
            }
        }

        private string _getMatrixForm()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < DecisionVariables; i++)
            {
                sb.AppendFormat("X{0}\t", i);
            }
            for (int i = 0; i < SlackVariables; i++)
            {
                sb.AppendFormat("S{0}\t", i);
            }
            for (int i = 0; i < ArtificialVariables; i++)
            {
                sb.AppendFormat("A{0}\t", i);
            }
            sb.AppendLine("|\tRHS");
            var j = 0;
            foreach (var constraint in LHS.EnumerateRows())
            {
                sb.Append(string.Join("\t", constraint.ToArray()));
                sb.AppendLine("\t|\t" + RHS.At(j++));
            }
            sb.AppendLine("Objective");
            sb.AppendLine(string.Join("\t", ObjectiveRow.Row(0).ToArray()));
            return sb.ToString();
        }

        private string _stringExpression(IEnumerable<double> terms, bool onlyShowDecisionVariables = false)
        {
            var i = 0;
            var expression = string.Join("\t+ ",
                terms.Take(this.DecisionVariables).Select(s =>
                    string.Format("{0} X{1}", s, ++i)
                ));
            if (SlackVariables > 0 && !onlyShowDecisionVariables)
            {
                var slackExpression = string.Join("\t+ ",
                    terms.Skip(DecisionVariables).Take(SlackVariables).Select(
                        s => string.Format("{0} S{1}", s, ++i - DecisionVariables)
                    )
                );
                expression = string.Format("{0}\t+ {1}", expression, slackExpression);
            }
            if (ArtificialVariables > 0 && !onlyShowDecisionVariables)
            {
                var artificalExpression = string.Join("\t+\t",
                    terms.Skip(DecisionVariables + SlackVariables).Select(
                        s => string.Format("{0} A{1}", s, ++i - DecisionVariables - SlackVariables)
                    )
                );
                expression = string.Format("{0}\t+ {1}", expression, artificalExpression);
            }
            return expression;
        }
        #endregion

    }
}
