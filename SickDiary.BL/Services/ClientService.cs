using MongoDB.Driver;
using SickDiary.DL.Entities;
using SickDiary.DL.Interfaces;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;

namespace SickDiary.BL.Services;

public class ClientService
{
    private readonly IMongoRepository<Client> _repository;

    public ClientService(IMongoRepository<Client> clientRepository)
    {
        _repository = clientRepository;
    }

    public async Task<Client> Login(string login, string password, ISession session)
    {
        var client = await _repository.Collection.Find(x => x.Login == login).FirstOrDefaultAsync();

        if (client == null || !BCrypt.Net.BCrypt.Verify(password, client.Pass))
        {
            throw new ApplicationException("Invalid login or password");
        }

        // Зберігаємо Id як string у сесії
        session.SetString("UserId", client.Id);

        return client;
    }

    public async Task<Client> SignUp(string login, string password, string fullName)
    {
        var existingClient = await _repository.Collection.Find(x => x.Login == login).FirstOrDefaultAsync();
        if (existingClient != null)
        {
            throw new ApplicationException("User with this login already exists.");
        }

        var client = new Client()
        {
            Login = login,
            Pass = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = fullName
        };

        await _repository.AddAsync(client);

        return client;
    }
}