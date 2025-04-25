using MongoDB.Driver;
using SickDiary.DL.Entities;

namespace SickDiary.DL.Interfaces;

public interface IMongoRepository<T> where T : class
{
    IMongoCollection<T> Collection { get; }
    Task<T> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(string id);
}