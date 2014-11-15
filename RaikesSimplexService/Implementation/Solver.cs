using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using RaikesSimplexService.Contracts;
using RaikesSimplexService.DataModel;
using MathNet.Numerics.LinearAlgebra;

namespace RaikesSimplexService.Implementation
{
    
    public class Solver : ISolver
    {
        public Solution Solve(Model model){
            throw new NotImplementedException();
        }

        public Model Standardize(Model unstandard)
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
            var goalCoeffs = new double[model.Goal.Coefficients.Count() + slackCount + artificialCount];
            model.Goal.Coefficients.CopyTo(goalCoeffs, 0);
            model.Goal.Coefficients = goalCoeffs;
            return model;
        }
    }
}
