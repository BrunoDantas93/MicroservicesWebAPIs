using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventsAPI.Models.MongoDB;

public class EventsType
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("Type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("IsAgeRestriction")]
    public bool IsAgeRestriction { get; set; } = false;

    [BsonElement("Disabled")]
    public bool Disabled { get; set; } = false;
}
