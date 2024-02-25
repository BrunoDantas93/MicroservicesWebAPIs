namespace CommunicationService.Models.Responses;

public class MessageResponse
{
    public string ChatID { get; set; } = string.Empty;

    public string SenderId { get; set; } = string.Empty;

    public string ReceiverId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime sendDate { get; set; } = DateTime.Now;

}
