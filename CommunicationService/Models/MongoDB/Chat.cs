using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using static CommunicationService.Helpers.Enumerated;

namespace CommunicationService.Models.MongoDB;

public class Chat
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("Name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("Type")]
    public ChatType Type { get; set; } = ChatType.Individual;

    [BsonElement("Participants")]
    public List<string> Participants { get; set;} = new List<string>();

    [BsonElement("CreationDate")]
    public DateTime CreationDate { get; set; } = DateTime.Now;

    [BsonElement("Messages")]
    public List<Message> Messages { get; set;} = new List<Message>();
}
