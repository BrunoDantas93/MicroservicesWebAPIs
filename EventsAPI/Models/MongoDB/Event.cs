using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using static EventsAPI.Helpers.Enumerated;

namespace EventsAPI.Models.MongoDB;

public class Event
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("Name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("Description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("EventDateTime")]
    public DateTime EventDateTime { get; set; } = DateTime.Now;

    [BsonElement("Address")]
    public string Address { get; set; } = string.Empty;

    [BsonElement("Latitude")]
    public double Latitude { get; set; } = 0;

    [BsonElement("Longitude")]
    public double Longitude { get; set; } = 0;

    [BsonElement("EventTypes")]
    public List<string> EventTypes { get; set; } = new List<string>();

    [BsonElement("State")]
    public EventState State { get; set; } = EventState.EmExecucao;

    [BsonElement("CreatedBy")]
    public string CreatedBy { get; set; } = string.Empty;

    [BsonElement("Participants")]
    public List<Participant> Participants { get; set; } = new List<Participant>();
}
