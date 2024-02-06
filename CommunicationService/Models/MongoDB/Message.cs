using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using static CommunicationService.Helpers.Enumerated;
using MongoDB.Bson.Serialization.IdGenerators;

namespace CommunicationService.Models.MongoDB;

public class Message
{
    [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("Content ")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("Timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("SenderId")]
    public string SenderId { get; set; }

    [BsonElement("ReceiverId")]
    public string ReceiverId { get; set; }

    [BsonElement("Status")]
    public MessageStatus Status { get; set; } = MessageStatus.Sent;
}
