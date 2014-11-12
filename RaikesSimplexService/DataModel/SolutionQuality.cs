using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace RaikesSimplexService.DataModel
{
    /// <summary>
    /// Enumeration for the quality of a solution.
    /// </summary>
    [DataContract]
    public enum SolutionQuality
    {
        [EnumMember]
        Optimal,
        [EnumMember]
        Infeasible,
        [EnumMember]
        Unbounded,
        [EnumMember]
        TimedOut
    }
}
