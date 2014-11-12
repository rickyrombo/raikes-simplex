using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace RaikesSimplexService.DataModel
{
    [DataContract]
    public class Solution
    {
        /// <summary>
        /// The optimized values for the decision variables
        /// </summary>
        [DataMember]
        public double[] Decisions { get; set; }

        /// <summary>
        /// The optimal value of the objective function.
        /// </summary>
        [DataMember]
        public double OptimalValue { get; set; }

        /// <summary>
        /// Set true if alternate solutions exist.
        /// </summary>
        [DataMember]
        public bool AlternateSolutionsExist { get; set; }

        /// <summary>
        /// The quality of the solution: optimal, infeasible, etc.
        /// </summary>
        [DataMember]
        public SolutionQuality Quality { get; set; }
        
    }
}

