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
                Decisions = new double[] { 0, 3.333333333333333, 0 }
            };
        }
    }
}
