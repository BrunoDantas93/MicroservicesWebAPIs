using System.ComponentModel.DataAnnotations;
using static CommunicationService.Helpers.Enumerated;

namespace CommunicationService.Models.SignalR;

public class MessageSignalR
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string SenderId { get; set; } = string.Empty;

    [Required]
    public string ReceiverId { get; set; } = string.Empty;

    [Required]
    public ChatType Type { get; set; } = ChatType.Individual;
}
