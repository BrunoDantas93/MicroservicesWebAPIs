using System.ComponentModel.DataAnnotations;
using static EventsAPI.Helpers.Enumerated;

namespace EventsAPI.Models.Requests;

public class EventRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime EventDateTime { get; set; }

    public string Address { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public EventState State { get; set; } = EventState.EmExecucao;

    [Required]
    public List<string> EventTypes { get; set; }
}
