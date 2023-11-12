
namespace MicroservicesHelpers.Models.CustomException;


/// <summary>
/// Custom exception for Microservices.
/// </summary>
internal class MicroservicesException : Exception
{
    private object response;
    private string additionalInfo;
    private bool returnResponse;

    /// <summary>
    /// Initializes a new instance of the <see cref="MicroservicesException"/> class.
    /// </summary>
    /// <param name="innerException">The exception caught from the code.</param>
    public MicroservicesException(Exception innerException)
        : base("", innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicroservicesException"/> class with additional information.
    /// </summary>
    /// <param name="innerException">The exception caught from the code.</param>
    /// <param name="additionalInfo">Additional information to be logged in case of an error.</param>
    public MicroservicesException(Exception innerException, string additionalInfo)
        : base("", innerException)
    {
        this.additionalInfo = additionalInfo;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicroservicesException"/> class with a response object.
    /// </summary>
    /// <param name="innerException">The exception caught from the code.</param>
    /// <param name="response">The response object to be returned to the client.</param>
    /// <param name="returnResponse">Flag indicating whether to return the response to the client.</param>
    public MicroservicesException(Exception innerException, object response, bool returnResponse = true)
        : base("", innerException)
    {
        this.response = response;
        this.returnResponse = returnResponse;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicroservicesException"/> class with additional information and a response object.
    /// </summary>
    /// <param name="innerException">The exception caught from the code.</param>
    /// <param name="additionalInfo">Additional information to be logged in case of an error.</param>
    /// <param name="response">The response object to be returned to the client.</param>
    /// <param name="returnResponse">Flag indicating whether to return the response to the client.</param>
    public MicroservicesException(Exception innerException, string additionalInfo, object response, bool returnResponse = true)
        : base("", innerException)
    {
        this.additionalInfo = additionalInfo;
        this.response = response;
        this.returnResponse = returnResponse;
    }

    /// <summary>
    /// Gets or sets the response object to be returned to the client.
    /// </summary>
    public object Response { get => response; set => response = value; }

    /// <summary>
    /// Gets or sets additional information to be logged in case of an error.
    /// </summary>
    public string AdditionalInfo { get => additionalInfo; set => additionalInfo = value; }

    /// <summary>
    /// Gets or sets a flag indicating whether to return the response to the client.
    /// </summary>
    public bool ReturnResponse { get => returnResponse; set => returnResponse = value; }

}
