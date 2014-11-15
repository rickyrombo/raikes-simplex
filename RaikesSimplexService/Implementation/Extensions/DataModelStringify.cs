using RaikesSimplexService.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaikesSimplexService.Implementation.Extensions
{
    public static class DataModelStringify
    {
        /// <summary>
        /// Turns a model into a string representation. Assumes all coefficients correspond to "normal" variables
        /// </summary>
        /// <param name="model">The LP model problem</param>
        /// <returns>The string representing the model problem</returns>
        public static string Stringify(this Model model)
        {
            return string.Format("Constraints:\n{0}, \nGoal:\n{1} {2}",
                string.Join("\n", model.Constraints.Select(s => s.Stringify())),
                model.GoalKind,
                model.Goal.Stringify());
        }

        /// <summary>
        /// Turns a model into a string representation. Needs to know the number of normal/slack variables so it can properly label coefficients
        /// </summary>
        /// <param name="model">The LP model problem</param>
        /// <param name="normalVariableCount">The number of "normal" variables (ie not slack/artificial)</param>
        /// <param name="slackVariableCount">The number of "slack" variables (slack or surplus)</param>
        /// <returns>The string representing the model program</returns>
        public static string Stringify(this Model model, int normalVariableCount, int slackVariableCount)
        {
            return string.Format("Constraints:\n{0}, \nGoal:\n{1} {2}",
                string.Join("\n", model.Constraints.Select(s => s.Stringify(normalVariableCount, slackVariableCount))),
                model.GoalKind,
                model.Goal.Stringify());
        }

        /// <summary>
        /// Turns a goal into string form by setting the expression of the coefficients equal to the constant term
        /// </summary>
        /// <param name="goal">the goal</param>
        /// <returns>The string representation of the goal</returns>
        public static string Stringify(this Goal goal)
        {
            return string.Format("{0} = {1}", goal.Coefficients.ToExpression(), goal.ConstantTerm);
        }

        /// <summary>
        /// Turns a constraint into string form by setting the expression of the coefficients related to the value by the proper symbol
        /// </summary>
        /// <param name="s">the constraint</param>
        /// <returns>The string representation of the constraint</returns>
        public static string Stringify(this LinearConstraint s)
        {
            return string.Format("{0} {1} {2}", s.Coefficients.ToExpression(), GetRelationshipSymbol(s.Relationship), s.Value);
        }

        /// <summary>
        /// Turns a constraint into string form by setting the expression of the coefficients (properly labeled) related to the value by the proper symbol
        /// </summary>
        /// <param name="s">the constraint</param>
        /// <param name="actualVariables">the number of "normal" variables (ie not slack/surplus/artificial)</param>
        /// <param name="slackVariables">the number of slack/surplus variables</param>
        /// <returns>The string representation of the constraint</returns>
        public static string Stringify(this LinearConstraint s, int actualVariables, int slackVariables)
        {
            return string.Format("{0} {1} {2}", s.Coefficients.ToExpression(actualVariables, slackVariables), GetRelationshipSymbol(s.Relationship), s.Value);
        }
        
        /// <summary>
        /// Turns an array of coefficients into an expression
        /// </summary>
        /// <param name="terms">the coefficients</param>
        /// <returns>The string representation of the expression, with each variable labeled x1, x2 x3 etc</returns>
        public static string ToExpression(this double[] terms)
        {
            var i = 0;
            return string.Join(" + ", terms.Select(s => string.Format("{0}x{1}", s, ++i)));
        }

        /// <summary>
        /// Turns an array of coefficients into an expression
        /// </summary>
        /// <param name="terms">the coefficients</param>
        /// <param name="actualVariables">the number of "normal" variables (to be labeled x1, x2, x3 ...)</param>
        /// <param name="slackVariables">the number of "slack" variables (to be labeled s1, s2, s3 ...) (the rest are a1, a2, a3...)</param>
        /// <returns>The string representation of the expression</returns>
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

        /// <summary>
        /// Gets the relationship symbol from the enum
        /// </summary>
        /// <param name="r">the enum relationship</param>
        /// <returns>">=" for greater than or equal, "<=" for less than or equal, or "=" for equal</returns>
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
