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

    public async Task AddRecordAsync(string userId, Record record)
    {
        // Перевіряємо, чи існує користувач
        var client = await _clientRepository.GetByIdAsync(userId);
        if (client == null)
        {
            throw new ApplicationException("User not found.");
        }

        // Шукаємо щоденник користувача
        var diary = await _diaryRepository.Collection
            .Find(d => d.UserId == userId)
            .FirstOrDefaultAsync();

        if (diary == null)
        {
            // Якщо щоденника немає, створюємо новий
            diary = new Diary
            {
                UserId = userId,
                Records = new List<Record>()
            };
            await _diaryRepository.AddAsync(diary);
        }

        // Додаємо новий запис до щоденника
        diary.Records.Add(record);

        // Оновлюємо щоденник у базі
        await _diaryRepository.UpdateAsync(diary);
    }

    //Додали UpdateRecordAsync, який замінює запис за індексом у списку Records.
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

        // Оновлюємо запис у щоденнику
        diary.Records[recordIndex] = updatedRecord;

        await _diaryRepository.UpdateAsync(diary);
    }

    // Додали DeleteRecordAsync, який видаляє запис за індексом із списку Records.
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

        if (diary == null || recordIndex < 0 || recordIndex >=diary.Records.Count)
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