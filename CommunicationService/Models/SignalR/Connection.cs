using CommunicationService.Models.SignalR;

/// <summary>
/// Class representing information associated with a connection.
/// </summary>
public class Connection
{
    /// <summary>
    /// Gets or sets the user ID associated with the connection.
    /// </summary>
    public string UserID { get; set; }

    /// <summary>
    /// Gets or sets the user name associated with the connection.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the language of the user associated with the connection.
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// Gets or sets the unique identifiers of the connection.
    /// </summary>
    public List<string> ConnectionIDs { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the list of conversation rooms associated with the connection.
    /// </summary>
    //public List<ConversationRoom> Rooms { get; set; 
    public List<string> Rooms { get; set; } = new List<string>();
}
