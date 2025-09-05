using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using MongoDB.Driver;

namespace Banking.Infrastructure.Repositories.Mongo;

public class MongoCustomerRepository : ICustomerRepository
{
    private readonly IMongoCollection<Customer> _customers;
    private readonly IMongoCollection<Account> _accounts;
    private readonly IMongoCollection<Transaction> _transactions;

    public MongoCustomerRepository(IMongoDatabase database)
    {
        _customers = database.GetCollection<Customer>("Customers");
        _accounts = database.GetCollection<Account>("Accounts");
        _transactions = database.GetCollection<Transaction>("Transactions");
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        // 1. Get customer
        var customer = await _customers
            .Find(c => c.Id == id)
            .FirstOrDefaultAsync(ct);

        if (customer == null)
            return null;

        // 2. Get accounts for this customer
        var accounts = await _accounts
            .Find(a => a.CustomerId == id)
            .ToListAsync(ct);

        if (!accounts.Any())
        {
            customer.Accounts = new List<Account>();
            return customer;
        }

        // 3. Get transactions for these accounts
        var accountIds = accounts.Select(a => a.Id).ToList();

        var transactions = await _transactions
            .Find(t => accountIds.Contains(t.AccountId))
            .ToListAsync(ct);

        // 4. Wire transactions into each account
        foreach (var account in accounts)
        {
            account.Transactions = transactions
                .Where(t => t.AccountId == account.Id)
                .ToList();
        }

        // 5. Wire accounts into customer
        customer.Accounts = accounts;

        return customer;
    }
}
