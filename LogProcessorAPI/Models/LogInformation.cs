using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using LogProcessorAPI.Helpers;
using System.Text.Json.Serialization;

namespace LogProcessorAPI.Models;

public class LogInformation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("WebAPI")]
    public string WebAPI { get; set; }

    [BsonElement("TimeStanp")]
    public DateTime TimeStanp { get; set; }

    [BsonElement("Value")]
    public Dictionary<string, object> Value { get; set; }

}
