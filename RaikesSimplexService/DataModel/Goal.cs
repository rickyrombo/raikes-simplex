using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace RaikesSimplexService.DataModel
{
    /// <summary>
    /// A goal has only one form, c1x1 + c2x2 + ... cnxn + d
    /// </summary>
    [DataContract]
    public class Goal
    {
        /// <summary>
        /// Data member that contains the coefficients of the constraint.
        /// </summary>
        [DataMember]
        public double[] Coefficients { get; set; }

        /// <summary>
        /// Data member that represents the constant term of the equation.
        /// </summary>
        [DataMember]
        public double ConstantTerm { get; set; }
    }
}
