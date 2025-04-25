using MongoDB.Bson.Serialization.Attributes;

namespace SickDiary.DL.Entities;

public class Diary : BaseEntity
{
    [BsonElement("userId")]
    public string UserId {  get; set; } = string.Empty; // Ідентифікатор користувача, якому належить щоденник

    [BsonElement("records")]
    public List<Record> Records { get; set; } = new List<Record>();
}

//UserId зв’язує щоденник із конкретним користувачем(Client.Id).
//Records ініціалізуємо як порожній список, щоб уникнути null.