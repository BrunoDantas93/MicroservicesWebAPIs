using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using static EventsAPI.Helpers.Enumerated;

namespace EventsAPI.Models.MongoDB;

public class Participant
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("Codigo")]
    public string Codigo { get; set; } = string.Empty;

    [BsonElement("ParticipantID")]
    public string ParticipantID { get; set; } = string.Empty;

    [BsonElement("Type")]
    public ParticipantType Type { get; set; }

    [BsonElement("DtType")]
    public DateTime DtType { get; set; }
    
    [BsonElement("Status")]
    public ParticipantStatus Status { get; set; }

    [BsonElement("DtStatus")]
    public DateTime DtStatus { get; set; }

}
