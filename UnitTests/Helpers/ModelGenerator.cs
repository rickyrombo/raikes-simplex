using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaikesSimplexService.DataModel;
using RaikesSimplexService.Implementation;

namespace UnitTests.Helpers
{
    public static class ModelGenerator
    {

        public static Model GetSimpleModel()
        {
            return new Model
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
            };
        }

        public static Model GetImpossibleModel()
        {
            return new Model
            {
                Constraints = new List<LinearConstraint>
                {
                    new LinearConstraint
                    {
                        Coefficients = new double[]{1},
                        Relationship = Relationship.LessThanOrEquals,
                        Value = 10,
                    },
                    new LinearConstraint
                    {
                        Coefficients = new double[]{1},
                        Relationship = Relationship.GreaterThanOrEquals,
                        Value = 20,
                    }
                },
                Goal = new Goal
                {
                    Coefficients = new double[] { 3 },
                    ConstantTerm = 0
                },
                GoalKind = GoalKind.Maximize
            };
        }

        public static Model GetUnboundedModel()
        {
            return new Model
            {
                Constraints = new List<LinearConstraint>
                {
                    new LinearConstraint
                    {
                        Coefficients = new double[]{2, 8},
                        Relationship = Relationship.GreaterThanOrEquals,
                        Value = 35.1,
                    }
                    
                },
                Goal = new Goal
                {
                    Coefficients = new double[] { 9, 9 },
                    ConstantTerm = 0
                },
                GoalKind = GoalKind.Maximize
            };
        }

        public static Model GetTwoPhaseModel()
        {
            return new Model
            {
                Constraints = new List<LinearConstraint>
                {
                    new LinearConstraint
                    {
                        Coefficients = new double[]{1, 1},
                        Relationship = Relationship.LessThanOrEquals,
                        Value = 1
                    },
                    new LinearConstraint
                    {
                        Coefficients = new double[]{2, -1},
                        Relationship = Relationship.GreaterThanOrEquals,
                        Value = 1
                    },
                    new LinearConstraint
                    {
                        Coefficients = new double[]{0, 3},
                        Relationship = Relationship.LessThanOrEquals,
                        Value = 2
                    }
                },
                Goal = new Goal
                {
                    Coefficients = new double[] {6, 3},
                    ConstantTerm = 0
                },
                GoalKind = GoalKind.Maximize
            };
        }
    }
}
