using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace LogProcessorAPI.Models;

public class LogInformation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("WebAPI")]
    public string WebAPI { get; set; }

    [BsonElement("Value")]
    public BsonDocument Value { get; set; }

}
