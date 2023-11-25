using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroservicesHelpers;

/// <summary>
/// Contains enumerated types used in the Microservices application.
/// </summary>
public class Enumerated
{
    /// <summary>
    /// Represents possible error codes for MicroservicesResponse.
    /// </summary>
    public enum MicroservicesCode
    {
        /// <summary>
        /// 
        /// </summary>
        NoError = 0,

        /// <summary>
        /// Indicates a successful operation.
        /// </summary>
        OK = 1,

        /// <summary>
        /// Indicates an error detected through validations.
        /// </summary>
        Validation = 2,

        /// <summary>
        /// Indicates a fatal error (catch).
        /// </summary>
        FatalError = 3
    }


    /// <summary>
    /// 
    /// </summary>
    public enum UserType
    {
        Admin = 0,
        User = 1
    }



}
