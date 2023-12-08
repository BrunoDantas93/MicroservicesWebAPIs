using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Models.Requests.User;

public class EventsTypeRequest
{
    [Required]
    public string Type { get; set; }

    public bool IsAgeRestriction { get; set; } = false;
}
