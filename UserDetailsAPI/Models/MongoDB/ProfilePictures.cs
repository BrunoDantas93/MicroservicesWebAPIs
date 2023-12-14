using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace UserDetailsAPI.Models.MongoDB;

public class ProfilePictures
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("ProfilePicture")]
    public byte[] ProfilePicture { get; set; }

}
