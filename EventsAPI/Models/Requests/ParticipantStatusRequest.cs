using System.ComponentModel.DataAnnotations;
using static EventsAPI.Helpers.Enumerated;

namespace EventsAPI.Models.Requests;

public class ParticipantStatusRequest
{
    [Required]
    public string ParticipantID { get; set; }

    [Required]
    public ParticipantStatus Status { get; set; }

    [Required]
    public ParticipantType type { get; set; }

    public string ParticipantEmail { get; set; }

    public string Codgio { get; set; }


}
