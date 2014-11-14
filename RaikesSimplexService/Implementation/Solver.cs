using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using RaikesSimplexService.Contracts;
using RaikesSimplexService.DataModel;

namespace RaikesSimplexService.Implementation
{
    
    public class Solver : ISolver
    {
        public Solution Solve(Model model)
        {
            throw new NotImplementedException();
        }

        public Model Standardize(Model nonstandard)
        {
            var model = new Model
            {
                Constraints = nonstandard.Constraints,
                Goal = nonstandard.Goal,
                GoalKind = nonstandard.GoalKind
            };
            //Minimize => Maximize
            if (model.GoalKind == GoalKind.Minimize)
            {
                model.Goal.Coefficients = model.Goal.Coefficients.Select(s => 0 - s).ToArray<double>();
                model.GoalKind = GoalKind.Maximize;
            }

            var artificialCount = model.Constraints.Count(s => s.Relationship == Relationship.GreaterThanOrEquals);
            var slackCount = model.Constraints.Count(s => s.Relationship == Relationship.LessThanOrEquals) + artificialCount;
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
                coeffs.AddRange(addedVariables);
                constraint.Coefficients = coeffs.ToArray<double>();
            }
            return model;
        }
    }
}
