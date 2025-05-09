﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SickDiary.DL.Entities;

public class BaseEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
}