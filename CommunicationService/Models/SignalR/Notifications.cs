using System.ComponentModel.DataAnnotations;

namespace CommunicationService.Models.SignalR;

public class Notifications
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    [Required]
    public List<string> UserID { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

}
