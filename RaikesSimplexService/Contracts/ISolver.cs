using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using RaikesSimplexService.DataModel;

namespace RaikesSimplexService.Contracts
{
    /// <summary>
    /// Provides a service to solve linnear optimization problem.
    /// </summary>
    [ServiceContract]
    public interface ISolver
    {
        //Solves a model that has been created and loaded. Solves the model using the specified directives.
        /// <summary>
        /// Solves a Model that is being passed into the function.
        /// </summary>
        /// <param name="model">Model that includes the goal equation, what to do with the goal equation, and constraint equations.</param>
        /// <returns>Returns a Solution object containing the optimized variable values along with solution quality information.</returns>
        [OperationContract]
        Solution Solve(Model model);

    }
}
