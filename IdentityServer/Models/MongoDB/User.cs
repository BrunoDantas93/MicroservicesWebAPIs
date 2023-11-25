﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MicroservicesHelpers;

namespace IdentityServer.Models.MongoDB;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("Username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("Password")]
    public string Password { get; set; } = string.Empty;

    [BsonElement("Email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("RegisterDate")]
    public DateTime RegisterDate { get; set; }

    [BsonElement("Usertype")]
    public Enumerated.UserType? UserType { get; set; }

    [BsonElement("RefreshToken")]
    public List<string> RefreshToken { get; set; } = new List<string>();
}