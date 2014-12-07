using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaikesSimplexService.DataModel;
using RaikesSimplexService.Implementation;

namespace UnitTests.Helpers
{
    public static class SolutionGenerator
    {
        public static Solution GetSimpleSolution()
        {
            return new Solution
            {
                AlternateSolutionsExist = true,
                OptimalValue = 20,
                Decisions = new double[] { 0, 3.333333333333333, 0 },
                Quality = SolutionQuality.Optimal
            };
        }

        public static Solution GetTwoPhaseSolution()
        {
            return new Solution
            {
                AlternateSolutionsExist = true,
                OptimalValue = 6,
                Decisions = new double[] { 1, 0 },
                Quality = SolutionQuality.Optimal
            };
        }

        public static Solution GetAshuSolution()
        {
            return new Solution
            {
                AlternateSolutionsExist = false,
                OptimalValue = .6,
                Quality = SolutionQuality.Optimal,
                Decisions = new double[] { 3, 0 }
            };
        }
    }
}
