using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaikesSimplexService.DataModel;

namespace RaikesSimplexService.Implementation.Extensions
{
    public static class DataModelEqualValues
    {
        public static bool EqualValues(this Goal self, Goal other)
        {
            bool sameCoefficients = self.Coefficients.EqualValues(other.Coefficients);
            bool sameConstantTerm = self.ConstantTerm == other.ConstantTerm;
            return sameCoefficients && sameConstantTerm;
        }

        public static bool EqualValues(this LinearConstraint self, LinearConstraint other)
        {
            bool sameCoefficients = self.Coefficients.EqualValues(other.Coefficients);
            bool sameRelationship = self.Relationship == other.Relationship;
            bool sameValue = self.Value == other.Value;
            return sameCoefficients && sameRelationship && sameValue;
        }

        public static bool EqualValues(this List<LinearConstraint> self, List<LinearConstraint> other)
        {
            var linConstraintPairs = self.Zip(other, (a, b) => new { First = a, Second = b });
            bool allPairsEqual = linConstraintPairs.All(pair => pair.First.EqualValues(pair.Second));
            return allPairsEqual;
        }

        public static bool EqualValues(this Model self, Model other)
        {
            bool sameGoal = self.Goal.EqualValues(other.Goal);
            bool sameConstraints = self.Constraints.EqualValues(other.Constraints);
            bool sameGoalKind = self.GoalKind == other.GoalKind;
            return sameGoal && sameConstraints && sameGoalKind;
        }

        public static bool EqualValues(this Solution self, Solution other)
        {
            bool sameOptimal = self.OptimalValue.NearlyEqual(other.OptimalValue);
            bool sameDecisions = self.Decisions.EqualValues(other.Decisions);
            bool sameQuality = self.Quality == other.Quality;
            bool sameAltSols = self.AlternateSolutionsExist == other.AlternateSolutionsExist
            return sameDecisions && sameQuality && sameOptimal && sameAltSols;
        }

        public static bool EqualValues(this double[] self, double[] other)
        {
            var pairs = self.Zip(other, (a, b) => new { First = a, Second = b });
            bool allPairsEqual = pairs.All(pair => pair.First.NearlyEqual(pair.Second));
            return allPairsEqual;
        }

    }
}
