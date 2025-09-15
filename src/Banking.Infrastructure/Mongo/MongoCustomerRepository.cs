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
        // Get customer
        var customer = await _customers
            .Find(c => c.Id == id)
            .FirstOrDefaultAsync(ct);

        if (customer == null)
            return null;

        //  Get accounts for this customer
        var accounts = await _accounts
            .Find(a => a.CustomerId == id)
            .ToListAsync(ct);

        if (!accounts.Any())
        {
            customer.Accounts = new List<Account>();
            return customer;
        }

        //  Get transactions for these accounts
        var accountIds = accounts.Select(a => a.Id).ToList();

        var transactions = await _transactions
            .Find(t => accountIds.Contains(t.AccountId))
            .ToListAsync(ct);

        //  Wire transactions into each account
        foreach (var account in accounts)
        {
            account.Transactions = transactions
                .Where(t => t.AccountId == account.Id)
                .ToList();
        }

        //  Wire accounts into customer
        customer.Accounts = accounts;

        return customer;
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await _customers
            .Find(c => c.Email == email)
            .FirstOrDefaultAsync(ct);
    }


    // Insert new customer (used by CustomerCreatedEvent handler)
    public async Task<Customer> AddAsync(Customer customer, CancellationToken ct)
    {
        await _customers.InsertOneAsync(customer, cancellationToken: ct);
        return customer;
    }

    // Update existing customer info (e.g. email change)
    public async Task UpdateAsync(Customer customer, CancellationToken ct)
    {
        await _customers.ReplaceOneAsync(
            c => c.Id == customer.Id,
            customer,
            new ReplaceOptions { IsUpsert = false },
            ct
        );
    }
   
    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return Task.FromResult(0);
    }
}
