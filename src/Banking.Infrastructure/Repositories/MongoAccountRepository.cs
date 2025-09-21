
using Banking.Domain.Entities;
using MongoDB.Driver;

public class MongoAccountRepository : IAccountRepository
{
    private readonly IMongoCollection<Account> _collection;
    private readonly IMongoCollection<Transaction> _transactions;

    public MongoAccountRepository(IMongoDatabase db)
    {
        _collection = db.GetCollection<Account>("Accounts");
        _transactions = db.GetCollection<Transaction>("Transactions");
    }

    public async Task<Account> AddAsync(Account account, CancellationToken ct)
    {
        await _collection.InsertOneAsync(account, cancellationToken: ct);
        return account;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var filter = Builders<Account>.Filter.Eq(a => a.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }
    public async Task<List<Account>> GetAccountsByCustomerIdAsync(Guid customerId, CancellationToken ct)
    {
 
        return await _collection.Find(a => a.CustomerId == customerId).ToListAsync(ct);
    }
    public async Task<Account?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct)
    {
        var filter = Builders<Account>.Filter.Eq(a => a.CustomerId, customerId);
        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task UpdateAsync(Account account, CancellationToken ct)
    {
        var filter = Builders<Account>.Filter.Eq(a => a.Id, account.Id);
        await _collection.ReplaceOneAsync(filter, account, cancellationToken: ct);
    }

    public async Task<decimal> AddTransactionAsync(Transaction transaction, CancellationToken ct)
    {
        //  Update the account balance
        var filter = Builders<Account>.Filter.Eq(a => a.Id, transaction.AccountId);
        var update = transaction.Type == TransactionType.Credit
            ? Builders<Account>.Update.Inc(a => a.Balance, transaction.Amount)
            : Builders<Account>.Update.Inc(a => a.Balance, -transaction.Amount);

        // Return the updated account after update
        var options = new FindOneAndUpdateOptions<Account>
        {
            ReturnDocument = ReturnDocument.After
        };
        var updatedAccount = await _collection.FindOneAndUpdateAsync(filter, update, options, ct);
        if (updatedAccount == null)
            throw new KeyNotFoundException("Account not found");

        // Insert the transaction into Transactions collection
        await _transactions.InsertOneAsync(transaction, cancellationToken: ct);

        //  Return the updated balance
        return updatedAccount.Balance;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        // MongoDB writes are immediate, no unit of work
        return Task.FromResult(0);
    }
}
