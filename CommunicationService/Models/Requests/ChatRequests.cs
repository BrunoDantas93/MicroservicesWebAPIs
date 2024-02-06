using System.ComponentModel.DataAnnotations;
using static CommunicationService.Helpers.Enumerated;
using CommunicationService.Models.MongoDB;

namespace CommunicationService.Models.Requests;

public class ChatRequests
{
    public ChatType Type { get; set; } = ChatType.Individual;

    public string Name { get; set; } = string.Empty;

    [Required]
    public List<string> Participants { get; set; } = new List<string>();

    public List<MessageRequests> Messages { get; set; } = new List<MessageRequests>();
}
