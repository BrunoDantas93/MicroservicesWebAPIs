namespace CommunicationService.Models.Responses;

public class MessageResponse
{
    /// <summary>
    /// Gets or sets the user name associated with the connection.
    /// </summary>
    public string UserName { get; set; } = string.Empty;


    public string Content { get; set; } = string.Empty;

    public DateTime sendDate { get; set; } = DateTime.Now;

}
