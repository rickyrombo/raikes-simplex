﻿using MathNet.Numerics.LinearAlgebra;
using RaikesSimplexService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaikesSimplexService.Implementation
{
    public class StandardModel
    {
        public Model OriginalModel { get; private set; }
        public Matrix<double> LHS { get; private set; }
        public Vector<double> RHS { get; private set; }
        public Matrix<double> ObjectiveRow { get; private set; }

        public int DecisionVariables { get; private set; }
        public int SlackVariables { get; private set; }
        public int ArtificialVariables { get; private set; }

        private StandardModel(int constraintCount, int decisionCount, int slackCount, int artificialCount)
        {
            var totalVars = decisionCount + slackCount + artificialCount;
            this.LHS = Matrix<double>.Build.Dense(constraintCount, totalVars);
            this.RHS = Vector<double>.Build.Dense(constraintCount);
            this.ObjectiveRow = Matrix<double>.Build.Dense(1, totalVars);
            this.DecisionVariables = decisionCount;
            this.SlackVariables = slackCount;
            this.ArtificialVariables = artificialCount;
        }

        public static StandardModel FromModel(Model model)
        {
            var artificialCount = model.Constraints.Count(s => s.Relationship == Relationship.GreaterThanOrEquals || s.Relationship == Relationship.Equals);
            var slackCount = model.Constraints.Count(s => s.Relationship == Relationship.LessThanOrEquals || s.Relationship == Relationship.GreaterThanOrEquals);

            var standardModel = new StandardModel(
                model.Constraints.Count(), 
                model.Constraints.First().Coefficients.Length,
                slackCount,
                artificialCount
            );

            standardModel.OriginalModel = model;

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
                standardModel.LHS.SetRow(i++, coeffs.ToArray<double>());
            }
            
            var goalCoeffs = model.Goal.Coefficients.ToArray<double>();
            Array.Resize<double>(ref goalCoeffs, goalCoeffs.Length + slackCount + artificialCount);
            if (model.GoalKind == GoalKind.Maximize)
            {
                standardModel.ObjectiveRow.SetRow(0, goalCoeffs.Select(s => 0 - s).ToArray<double>());
            } else {
                standardModel.ObjectiveRow.SetRow(0, goalCoeffs);
            }
            standardModel.RHS.SetValues(model.Constraints.Select(c => c.Value).ToArray<double>());
            return standardModel;
        }

        private string ConstraintExpression(Vector<double> terms)
        {
            var i = 0;
            var expression = string.Join("\t+ ",
                terms.Take(this.DecisionVariables).Select(s => string.Format("{0} X{1}", s, ++i)));
            if (SlackVariables > 0)
            {
                var slackExpression = string.Join("\t+ ",
                    terms.Skip(DecisionVariables).Take(SlackVariables).Select(
                        s => string.Format("{0} S{1}", s, ++i - DecisionVariables)
                    )
                );
                expression = string.Format("{0}\t+ {1}", expression, slackExpression);
            }
            if (ArtificialVariables > 0)
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
      
        public override string ToString()
        {
            return string.Format("LHS:\n{0}\n\nRHS:\n{1}\n\nObjective Row:\n{2}", this.LHS, this.RHS, this.ObjectiveRow);
        }


        public string ToString(bool matrixForm)
        {
            if (!matrixForm)
            {
                return string.Format("Constraints:\n{0}\nObjective:\nMaximize Z {1} = 0",
                    string.Join("\n", LHS.EnumerateRowsIndexed().Select(
                        r => string.Format("{0} = {1}", ConstraintExpression(r.Item2), this.RHS.At(r.Item1))
                    )),
                    ConstraintExpression(ObjectiveRow.Row(0))
                );
            }
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
    }
}