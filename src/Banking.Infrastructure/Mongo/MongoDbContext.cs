using MongoDB.Driver;
using Banking.Domain.Entities;

namespace Banking.Infrastructure.Mongo;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>("Customers");
    public IMongoCollection<Account> Accounts => _database.GetCollection<Account>("Accounts");
    public IMongoCollection<Transaction> Transactions => _database.GetCollection<Transaction>("Transactions");
}
