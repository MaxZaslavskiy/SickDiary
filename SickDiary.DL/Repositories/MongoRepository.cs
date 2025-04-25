using MongoDB.Driver;
using SickDiary.DL.Interfaces;

namespace SickDiary.DL.Repositories;

public class MongoRepository<T> : IMongoRepository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public MongoRepository(IMongoDatabase database)
    {
        var collectionName = typeof(T).Name;
        _collection = database.GetCollection<T>(collectionName);
    }

    public IMongoCollection<T> Collection => _collection;

    public async Task AddAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        var id = entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString();
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Entity Id cannot be null or empty");
        }

        var filter = Builders<T>.Filter.Eq("Id", id);
        await _collection.ReplaceOneAsync(filter, entity);
    }

    public async Task<T> GetByIdAsync(string id)
    {
        return await _collection.Find(Builders<T>.Filter.Eq("Id", id)).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("Id", id));
    }
}