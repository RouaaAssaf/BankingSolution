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

    public async Task<int> CountTransactionsAsync(Func<Transaction, bool>? filter, CancellationToken ct)
    {
        if (filter == null)
            return (int)await _transactions.CountDocumentsAsync(FilterDefinition<Transaction>.Empty, cancellationToken: ct);

        var all = await _transactions.Find(_ => true).ToListAsync(ct);
        return all.Count(filter);
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

    public async Task DeleteAsync(Guid transactionId, CancellationToken ct)
    {
        await _transactions.DeleteOneAsync(t => t.Id == transactionId, ct);
    }

    public async Task DeleteByAccountIdAsync(Guid accountId, CancellationToken ct)
    {
        await _transactions.DeleteManyAsync(t => t.AccountId == accountId, ct);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken ct)
    {
        await _transactions.InsertOneAsync(transaction, cancellationToken: ct);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct) =>
        Task.FromResult(0);
}
