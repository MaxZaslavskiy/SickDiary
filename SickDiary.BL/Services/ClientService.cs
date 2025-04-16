using MongoDB.Driver;
using SickDiary.DL.Entities;
using SickDiary.DL.Interfaces;

namespace SickDiary.BL.Services;

public class ClientService
{
    private IMongoRepository<Client> _repository;
    public ClientService(IMongoRepository<Client> clientRepository)
    {
        _repository = clientRepository;
    }

    public async Task<Client> Login(string login, string password)
    {
        var client = await _repository.Collection.Find(x=> x.Login == login && x.Pass == password).FirstOrDefaultAsync();
        
        if (client != null)
            return client;
        
        throw new ApplicationException("Invalid login or password");
    }
    
    public async Task<Client> SignUp(string login, string password, string fullName)
    {
        var client = new Client()
        {
            Login = login,
            Pass = password,
            FullName = fullName
        };
        
        await _repository.AddAsync(client);

        return client;
    }
}