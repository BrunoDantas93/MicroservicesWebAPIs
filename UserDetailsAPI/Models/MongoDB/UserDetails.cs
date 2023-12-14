using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserDetailsAPI.Models.MongoDB;

public class UserDetails
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("FirstName")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("LastName")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("Address")]
    public string Address { get; set; } = string.Empty;

    [BsonElement("Nationality")]
    public string Nationality { get; set; } = string.Empty;

    [BsonElement("BirthDate")]
    public DateOnly BirthDate { get; set; }

    [BsonElement("Gender")]
    public string Gender { get; set; } = string.Empty;


}
