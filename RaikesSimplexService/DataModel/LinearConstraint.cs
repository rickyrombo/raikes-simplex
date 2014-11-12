using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace RaikesSimplexService.DataModel
{
    /// <summary>
    /// A linear constraint has the form of...
    /// c1x1 + ... cnxn = v
    /// c1x1 + ... cnxn <= v
    /// c1x1 + ... cnxn >= v
    /// </summary>
    [DataContract]
    public class LinearConstraint
    {
        /// <summary>
        /// Data member that contains the coefficients of the constraint.
        /// </summary>
        [DataMember]
        public double[] Coefficients { get; set; }

        /// <summary>
        /// Data member that represents the relationship between the left-hand side and right-hand side of the equation.
        /// </summary>
        [DataMember]
        public Relationship Relationship { get; set; }

        /// <summary>
        /// Constant value on the right-hand side of the equation.
        /// </summary>
        [DataMember]
        public double Value { get; set; }
    }

    /// <summary>
    /// Enumeration that represents the various relationships between the two halves of the equation.
    /// </summary>
    [DataContract]
    public enum Relationship
    {
        [EnumMember]
        Equals,
        [EnumMember]
        GreaterThanOrEquals,
        [EnumMember]
        LessThanOrEquals
    }
}
