using System.ComponentModel.DataAnnotations;

namespace CommunicationService.Models.Requests;

public class MessageRequests
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string SenderId { get; set; } = string.Empty;

    [Required]
    public string ReceiverId { get; set; } = string.Empty;
}
