using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaikesSimplexService.Implementation.Extensions
{
    public static class Delta
    {
        private static readonly double TOLERANCE = 0.00000001;

        public static bool NearlyZero(this double d)
        {
            return NearlyEqual(d, 0.0);
        }

        public static bool NearlyEqual(this double self, double other)
        {
            return self >= other - TOLERANCE && self <= other + TOLERANCE;
        }
    }
}
