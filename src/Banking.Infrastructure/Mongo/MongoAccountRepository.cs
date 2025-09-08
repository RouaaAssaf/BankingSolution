using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Banking.Infrastructure.Repositories.Mongo;

public class MongoAccountRepository : IAccountRepository
{
    private readonly IMongoCollection<Account> _accounts;
    private readonly IMongoCollection<Transaction> _transactions;

    public MongoAccountRepository(IMongoDatabase database)
    {
        _accounts = database.GetCollection<Account>("Accounts");
        _transactions = database.GetCollection<Transaction>("Transactions");
    }

    // Get a single account by Id
    public async Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct)
    {
        return await _accounts
            .Find(a => a.Id == accountId)
            .FirstOrDefaultAsync(ct);
    }

    // Add a new account
    public async Task<Account> AddAsync(Account account, CancellationToken ct)
    {
        await _accounts.InsertOneAsync(account, cancellationToken: ct);
        return account;
    }

    public async Task AddTransactionAsync(Transaction transaction, CancellationToken ct)
    {
        await _transactions.InsertOneAsync(transaction, cancellationToken: ct);

        var amountChange = transaction.Type == TransactionType.Credit
            ? transaction.Amount
            : -transaction.Amount;

        var update = Builders<Account>.Update
            .Inc("Balance", amountChange) // increment persisted balance
            .Push(a => a.Transactions, transaction); // also push the transaction

        await _accounts.UpdateOneAsync(
            Builders<Account>.Filter.Eq(a => a.Id, transaction.AccountId),
            update,
            cancellationToken: ct
        );
    }




    // Optional: get all accounts for a customer
    public async Task<List<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct)
    {
        return await _accounts
            .Find(a => a.CustomerId == customerId)
            .ToListAsync(ct);
    }

    // SaveChangesAsync is not needed for Mongo, but must be here to satisfy interface
    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return Task.FromResult(0);
    }
}
