using Newtonsoft.Json;
using static MicroservicesHelpers.Enumerated;

namespace MicroservicesHelpers.Models;

/// <summary>
/// Represents a response from Microservices.
/// </summary>
public class MicroservicesResponse
{
    private MicroservicesCode code;
    private string designation;
    private string description;
    private object data;

    /// <summary>
    /// Initializes a new instance of the <see cref="MicroservicesResponse"/> class.
    /// </summary>
    /// <param name="code">Error code associated with the response.</param>
    /// <param name="designation">A very brief designation for the error (Caption of a MessageBox).</param>
    /// <param name="description">A detailed description of the detected error (Message for the user).</param>
    /// <param name="data">Data to be transported to the client-side, including objects, lists, dictionaries.
    /// Avoid using large lists or data tables; use Description for error messages.</param>
    public MicroservicesResponse(MicroservicesCode code, string designation, string description, object data)
    {
        this.code = code;
        this.designation = designation;
        this.description = description;
        this.data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicroservicesResponse"/> class with default values.
    /// </summary>
    public MicroservicesResponse()
    {

    }

    /// <summary>
    /// Gets or sets the error code associated with the response.
    /// </summary>
    public MicroservicesCode Code { get => code; set => code = value; }

    /// <summary>
    /// Gets or sets a very brief designation for the error (Caption of a MessageBox).
    /// </summary>
    public string Designation { get => designation; set => designation = value; }

    /// <summary>
    /// Gets or sets a detailed description of the detected error (Message for the user).
    /// </summary>
    public string Description { get => description; set => description = value; }

    /// <summary>
    /// Gets or sets data to be transported to the client-side, including objects, lists, dictionaries.
    /// Avoid using large lists or data tables; use Description for error messages.
    /// </summary>
    public object Data { get => data; set => data = value; }

    /// <summary>
    /// Converts the MicroservicesResponse instance to a JSON string.
    /// </summary>
    /// <returns>JSON representation of the object.</returns>
    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }


    /// <summary>
    /// Initializes a new instance of the MicroservicesResponse class from a JSON string.
    /// </summary>
    /// <param name="json">JSON representation of the object.</param>
    /// <returns>An instance of MicroservicesResponse.</returns>
    public static MicroservicesResponse FromJson(string json)
    {
        return JsonConvert.DeserializeObject<MicroservicesResponse>(json);
    }

}
