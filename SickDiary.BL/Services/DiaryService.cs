using MongoDB.Driver;
using SickDiary.DL.Entities;
using SickDiary.DL.Interfaces;

namespace SickDiary.BL.Services;

public class DiaryService
{
    private readonly IMongoRepository<Diary> _diaryRepository;
    private readonly IMongoRepository<Client> _clientRepository;

    public DiaryService(IMongoRepository<Diary> diaryRepository, IMongoRepository<Client> clientRepository)
    {
        _diaryRepository = diaryRepository;
        _clientRepository = clientRepository;
    }

    private DiseaseState CalculateDiseaseState(Record record)
    {
        // Норми для рівня глюкози
        double glucoseLevel = record.BloodGlucoseLevel;
        bool isFasting = record.MeasurementState == MeasurementState.Fasting;
        int carbIntake = record.CarbohydrateIntake;
        double insulinDose = record.InsulinDose;
        PhysicalActivityLevel activityLevel = record.PhysicalActivityLevel;
        WellBeingLevel wellBeing = record.WellBeingLevel;

        // Критерії для глюкози
        bool isHypoglycemic = isFasting ? glucoseLevel < 3.9 : glucoseLevel < 5.0;
        bool isHyperglycemic = isFasting ? glucoseLevel > 7.2 : glucoseLevel > 10.0;
        bool isNormal = !isHypoglycemic && !isHyperglycemic;

        // Кількість симптомів
        int symptomCount = 0;
        if (record.Dizziness) symptomCount++;
        if (record.Sweating) symptomCount++;
        if (record.VisionProblems) symptomCount++;
        if (record.Weakness) symptomCount++;

        // Додаткові фактори
        bool highCarbIntake = carbIntake > 100; // Високе споживанння вуглеводів
        bool lowInsulinForHighGlucose = insulinDose < 5 && isHyperglycemic; // Недостатня кількість інсуліну при гіперклікемії
        bool highActivityRisk = activityLevel == PhysicalActivityLevel.High && glucoseLevel < 5.0; // Висока активність при низькому рівні глюкози

        // Логіка визначення стану
        if (isHypoglycemic)
        {
            if (symptomCount >= 2 || highActivityRisk)
            {
                return DiseaseState.Critical; // Критичний стан: гіпоглікемія + симптоми або висока активність
            }
            return DiseaseState.Warn; // Попередження: гіпоглікемія
        }

        if (isHyperglycemic)
        {
            if (symptomCount >= 2 || highCarbIntake || lowInsulinForHighGlucose)
            {
                return DiseaseState.Critical; // Критичний стан: гіперглікемія + симптоми, багато вуглеводів або мало інсуліну
            }
            return DiseaseState.Warn;
        }

        if (isNormal)
        {
            if (symptomCount >= 2 || (highCarbIntake && insulinDose < 5))
            {
                return DiseaseState.Warn; // Попередження: нормальний рівень глюкози, але є ризики
            }
            if (symptomCount == 1 || wellBeing == WellBeingLevel.Bad || wellBeing == WellBeingLevel.VeryBad)
            {
                return DiseaseState.Normal; // Нормальний стан: є 1 симптом або погане самопочуття
            }
            if (wellBeing == WellBeingLevel.VeryGood && symptomCount == 0)
            {
                return DiseaseState.VeryGood; // Дуже хороший стан
            }
            if (wellBeing == WellBeingLevel.Good && symptomCount == 0)
            {
                return DiseaseState.Good; // Хороший стан
            }
            return DiseaseState.Normal; // Нормальний стан за замовчуванням
        }

        return DiseaseState.Normal; // За замовчуванням
    }

    public async Task AddRecordAsync(string userId, Record record)
    {
        var client = await _clientRepository.GetByIdAsync(userId);
        if (client == null)
        {
            throw new ApplicationException("User not found.");
        }

        var diary = await _diaryRepository.Collection
            .Find(d => d.UserId == userId)
            .FirstOrDefaultAsync();

        if (diary == null)
        {
            diary = new Diary
            {
                UserId = userId,
                Records = new List<Record>()
            };
            await _diaryRepository.AddAsync(diary);
        }

        // Автоматично визначаємо DiseaseState перед додаванням
        record.Result = CalculateDiseaseState(record);
        diary.Records.Add(record);

        await _diaryRepository.UpdateAsync(diary);
    }

    public async Task UpdateRecordAsync(string userId, int recordIndex, Record updatedRecord)
    {
        var client = await _clientRepository.GetByIdAsync(userId);
        if (client == null)
        {
            throw new ApplicationException("User not found.");
        }

        var diary = await _diaryRepository.Collection
            .Find(d => d.UserId == userId)
            .FirstOrDefaultAsync();

        if (diary == null || recordIndex < 0 || recordIndex >= diary.Records.Count)
        {
            throw new ApplicationException("Record not found");
        }

        // Автоматично визначаємо DiseaseState перед оновленням
        updatedRecord.Result = CalculateDiseaseState(updatedRecord);
        diary.Records[recordIndex] = updatedRecord;

        await _diaryRepository.UpdateAsync(diary);
    }

    public async Task DeleteRecordAsync(string userId, int recordIndex)
    {
        var client = await _clientRepository.GetByIdAsync(userId);
        if (client == null)
        {
            throw new ApplicationException("User not found.");
        }

        var diary = await _diaryRepository.Collection
            .Find(d => d.UserId == userId)
            .FirstOrDefaultAsync();

        if (diary == null || recordIndex < 0 || recordIndex >= diary.Records.Count)
        {
            throw new ApplicationException("Record not found.");
        }

        diary.Records.RemoveAt(recordIndex);
        await _diaryRepository.UpdateAsync(diary);
    }

    public async Task<Diary> GetDiaryByUserIdAsync(string userId)
    {
        var client = await _clientRepository.GetByIdAsync(userId);
        if (client == null)
        {
            throw new ApplicationException("User not found.");
        }

        return await _diaryRepository.Collection
            .Find(d => d.UserId == userId)
            .FirstOrDefaultAsync();
    }
}
