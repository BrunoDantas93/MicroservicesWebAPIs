using System.ComponentModel.DataAnnotations;

namespace EventsAPI.Models.Requests;

public class InviteParticipantRequest
{
    public string ParticipantID { get; set; }
       
    public List<string> ParticipantTokens { get; set; }
    
    public string ParticipantEmail { get; set; }

    [Required]
    public string Title { get; set; }

    [Required]
    public string Body { get; set; }
}
