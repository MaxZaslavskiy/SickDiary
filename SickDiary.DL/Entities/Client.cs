using MongoDB.Bson.Serialization.Attributes;

namespace SickDiary.DL.Entities;

public class Client : BaseEntity
{
    [BsonElement("login")]
    public string Login { get; set; }
    
    [BsonElement("pass")]
    public string Pass { get; set; }
    
    [BsonElement("fullName")]
    public string FullName { get; set; }
    
    [BsonElement("disease")]
    public string Disease  { get; set; }
    
    [BsonElement("aboutDiseaseState")]
    [BsonIgnoreIfNull]
    public string? AboutDiseaseState { get; set; }
}