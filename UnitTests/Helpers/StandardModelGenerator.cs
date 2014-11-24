using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaikesSimplexService.DataModel;
using RaikesSimplexService.Implementation;

namespace UnitTests.Helpers
{
    public static class StandardModelGenerator
    {
        public static StandardModel getSimpleModel()
        {
            return StandardModel.FromModel(new Model
            {
                Constraints = new List<LinearConstraint>
                { 
                    new LinearConstraint{
                        Coefficients = new double[]{1, 3, 4},
                        Relationship = Relationship.LessThanOrEquals,
                        Value = 10,
                    },
                    new LinearConstraint{
                        Coefficients = new double[]{4, 3, 2},
                        Relationship = Relationship.LessThanOrEquals,
                        Value = 50,
                    }
                },
                Goal = new Goal
                {
                    Coefficients = new double[] { 2, 6, 4 },
                    ConstantTerm = 20
                },
                GoalKind = GoalKind.Maximize
            });
        }

        public static StandardModel getStandardizedSimpleModel()
        {
            return StandardModel.FromModel(new Model
            {
                Constraints = new List<LinearConstraint>
                { 
                    new LinearConstraint{
                        Coefficients = new double[]{1, 3, 4, 1, 0},
                        Relationship = Relationship.Equals,
                        Value = 10,
                    },
                    new LinearConstraint{
                        Coefficients = new double[]{4, 3, 2, 0, 1},
                        Relationship = Relationship.Equals,
                        Value = 50,
                    }
                },
                Goal = new Goal
                {
                    Coefficients = new double[] { 2, 6, 4, 0, 0 },
                    ConstantTerm = 20
                },
                GoalKind = GoalKind.Maximize
            });
        }
    }
}
