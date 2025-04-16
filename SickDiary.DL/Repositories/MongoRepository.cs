using MongoDB.Driver;
using SickDiary.DL.Entities;
using SickDiary.DL.Interfaces;

namespace SickDiary.DL.Repositories;

public class MongoRepository<T> : IMongoRepository<T> where T : BaseEntity
{
    private readonly IMongoCollection<T> _collection;
    public IMongoCollection<T> Collection
    {
        get => _collection;
    }

    public MongoRepository(IMongoDatabase  database)
    {
        _collection = database.GetCollection<T>(typeof(T).Name);
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        // Знаходимо максимальний Id у колекції
        var maxId = (await _collection.Find(_ => true)
            .SortByDescending(x => x.Id)
            .FirstOrDefaultAsync())?.Id ?? 0;

        // Присвоюємо новому об’єкту Id, який на 1 більший
        entity.Id = maxId + 1;

        // Вставляємо документ у колекцію
        await _collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
    }

    public async Task DeleteAsync(int id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }
}