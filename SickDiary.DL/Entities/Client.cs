using MongoDB.Bson.Serialization.Attributes;

namespace SickDiary.DL.Entities;

public class Client : BaseEntity
{
    [BsonElement("login")]
    public string Login { get; set; } = string.Empty;

    [BsonElement("pass")]
    public string Pass { get; set; } = string.Empty;

    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

}
