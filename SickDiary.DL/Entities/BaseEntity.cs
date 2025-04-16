using MongoDB.Bson.Serialization.Attributes;

namespace SickDiary.DL.Entities;

public class BaseEntity
{
    [BsonId]
    public int Id { get; set; }
}