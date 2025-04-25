using SickDiary.DL.Entities;

namespace SickDiary.Web.Models
{
    public class RecordViewModel
    {

        public int Index { get; set; } // Додаємо поле для індексу
        public double BloodGlucoseLevel { get; set; }

        public double InsulinDose {  get; set; }

        public int CarbohydrateIntake { get; set; }

        public WellBeingLevel WellBeingLevel { get; set; }

        public PhysicalActivityLevel PhysicalActivityLevel { get; set; }

        public bool Dizziness { get; set; }

        public bool Sweating { get; set; }

        public bool VisionProblems {  get; set; }

        public bool Weakness { get; set; }

        public DateTime Date { get; set; } = DateTime.Now; // За замовчуванням сьогоднішня дата

        public DiseaseState Result { get; set; }
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
}
