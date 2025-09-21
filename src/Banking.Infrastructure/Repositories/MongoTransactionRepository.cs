using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using MongoDB.Driver;

namespace Banking.Infrastructure.Repositories.Mongo;

public class MongoTransactionRepository : ITransactionRepository
{
    private readonly IMongoCollection<Transaction> _transactions;

    public MongoTransactionRepository(IMongoDatabase database)
    {
        _transactions = database.GetCollection<Transaction>("Transactions");
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken ct)
    {
        return await _transactions.Find(t => t.AccountId == accountId).ToListAsync(ct);
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdsAsync(IEnumerable<Guid> accountIds, CancellationToken ct)
    {
        return await _transactions
            .Find(t => accountIds.Contains(t.AccountId))
            .ToListAsync(ct);
    }



    public async Task AddAsync(Transaction transaction, CancellationToken ct)
    {
        await _transactions.InsertOneAsync(transaction, cancellationToken: ct);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct) =>
        Task.FromResult(0);
}
