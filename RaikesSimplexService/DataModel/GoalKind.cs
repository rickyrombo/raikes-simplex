using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace RaikesSimplexService.DataModel
{
    /// <summary>
    /// Enumeration that defines the direction of the goal.
    /// </summary>
    [DataContract]
    public enum GoalKind
    {
        [EnumMember]
        Maximize,
        [EnumMember]
        Minimize
    }
}
