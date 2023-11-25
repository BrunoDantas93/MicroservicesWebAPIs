using MongoDB.Bson.Serialization.Attributes;

namespace IdentityServer.Models.MongoDB;

public class PasswordRecovery
{
    [BsonElement("RecoveryCode")]
    public string RecoveryCode { get; set; } = string.Empty;

    [BsonElement("ExpirationTime")]
    public DateTime ExpirationTime { get; set; }
}
