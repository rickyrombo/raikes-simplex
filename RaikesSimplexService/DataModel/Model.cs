using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;


namespace RaikesSimplexService.DataModel
{
    /// <summary>
    /// Defines a model that has expressions and constraints.
    /// </summary>
    [DataContract]
    public class Model
    {
        /// <summary>
        /// Data member that contains the function you want to optimize.
        /// </summary>
        [DataMember]
        public Goal Goal { get; set; }

        /// <summary>
        /// Data member that contains a list of constraint equations.
        /// </summary>
        [DataMember]
        public List<LinearConstraint> Constraints { get; set; }

        /// <summary>
        /// Data member that indicated whether to maximize of minimize the goal function.
        /// </summary>
        [DataMember]
        public GoalKind GoalKind { get; set; }
    }
}
