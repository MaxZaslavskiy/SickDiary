using MongoDB.Bson.Serialization.Attributes;

namespace SickDiary.DL.Entities;

public class Record
{
    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("bloodGlucoseLevel")]
    public double BloodGlucoseLevel { get; set; } // Рівень цукру в ммоль/л

    [BsonElement("insulinDose")]
    public double InsulinDose { get; set; } // Доза інсуліну в одиницях

    [BsonElement("carbohydrateIntake")]
    public int CarbohydrateIntake { get; set; } // Кількість вуглеводів у грамах

    [BsonElement("wellBeingLevel")]
    public WellBeingLevel WellBeingLevel { get; set; } // Самопочуття

    [BsonElement("physicalActivityLevel")]
    public PhysicalActivityLevel PhysicalActivityLevel { get; set; } // Фізична активність

    [BsonElement("dizziness")]
    public bool Dizziness { get; set; } // Запаморочення

    [BsonElement("sweating")]
    public bool Sweating { get; set; } // Пітливість

    [BsonElement("visionProblems")]
    public bool VisionProblems { get; set; } // Проблеми із зором

    [BsonElement("weakness")]
    public bool Weakness { get; set; } // Слабкість

    [BsonElement("result")]
    public DiseaseState Result { get; set; } // Загальний стан (може визначатися автоматично)
}

public enum WellBeingLevel
{
    VeryBad = 0,
    Bad = 1,
    Normal = 2,
    Good = 3,
    VeryGood = 4
}

public enum PhysicalActivityLevel
{
    Low = 0,
    Normal = 1,
    High = 2
}

public enum DiseaseState
{
    Critical = 0,
    Warn = 1,
    Normal = 2,
    Good = 3,
    VeryGood = 4
}