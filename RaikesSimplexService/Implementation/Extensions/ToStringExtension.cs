using RaikesSimplexService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaikesSimplexService.Implementation.Extensions
{
    public static class ToStringExtension
    {
        public static string Stringify(this Model model, int normalVariableCount, int slackVariableCount)
        {
            return string.Format("Constraints:\n{0}, \nGoal:\n{1} {2}",
                string.Join("\n", model.Constraints.Select(s => s.Stringify(normalVariableCount, slackVariableCount))),
                model.GoalKind,
                model.Goal.Stringify());
        }
        public static string Stringify(this Model model)
        {
            return string.Format("Constraints:\n{0}, \nGoal:\n{1} {2}",
                string.Join("\n", model.Constraints.Select(s => s.Stringify())),
                model.GoalKind,
                model.Goal.Stringify());
        }

        public static string Stringify(this Goal goal)
        {
            return string.Format("{0} = {1}", goal.Coefficients.ToExpression(), goal.ConstantTerm);
        }

        public static string Stringify(this LinearConstraint s)
        {
            return string.Format("{0} {1} {2}", s.Coefficients.ToExpression(), GetRelationshipSymbol(s.Relationship), s.Value);
        }
        public static string Stringify(this LinearConstraint s, int actualVariables, int slackVariables)
        {
            return string.Format("{0} {1} {2}", s.Coefficients.ToExpression(actualVariables, slackVariables), GetRelationshipSymbol(s.Relationship), s.Value);
        }

        public static string ToExpression(this double[] terms)
        {
            var i = 0;
            return string.Join(" + ", terms.Select(s => string.Format("{0}x{1}", s, ++i)));
        }
        public static string ToExpression(this double[] terms, int actualVariables, int slackVariables)
        {
            var i = 0;
            var actualExpression = string.Join(" + ", 
                terms.Take(actualVariables).Select(s => string.Format("{0}x{1}", s, ++i)));
            var slackExpression = string.Join(" + ", 
                terms.Skip(actualVariables).Take(slackVariables).Select(
                    s => string.Format("{0}s{1}", s, ++i - actualVariables)
                )
            );
            var artificalExpression = string.Join(" + ", 
                terms.Skip(actualVariables + slackVariables).Select(
                    s => string.Format("{0}a{1}", s, ++i - actualVariables - slackVariables)
                )
            );
            return string.Format("{0} + {1} + {2}", actualExpression, slackExpression, artificalExpression);
        }

        public static string GetRelationshipSymbol(Relationship r)
        {
            switch (r)
            {
                case Relationship.Equals:
                    return "=";
                case Relationship.GreaterThanOrEquals:
                    return ">=";
                default:
                    return "<=";
            }
        }
    }
}
