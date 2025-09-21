using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using MongoDB.Driver;


namespace Banking.Infrastructure.Repositories.Mongo
{
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
            // Find customer by Id
            var customer = await _customers.Find(c => c.Id == id).FirstOrDefaultAsync(ct);
            if (customer == null) return null;

            //  Get accounts
            var accounts = await _accounts.Find(a => a.CustomerId == id).ToListAsync(ct);
            customer.Accounts = accounts;

            return customer;
        }

        public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct)
        {
            return await _customers.Find(c => c.Email == email).FirstOrDefaultAsync(ct);
        }

        public async Task UpdateBalanceAsync(Guid customerId, decimal newBalance, CancellationToken ct)
        {
            var update = Builders<Customer>.Update.Set(c => c.Balance, newBalance);
            await _customers.UpdateOneAsync(c => c.Id == customerId, update, cancellationToken: ct);
        }

        public async Task<Customer> AddAsync(Customer customer, CancellationToken ct)
        {
            await _customers.InsertOneAsync(customer, cancellationToken: ct);
            return customer;
        }

        public async Task UpdateAsync(Customer customer, CancellationToken ct)
        {
            await _customers.ReplaceOneAsync(c => c.Id == customer.Id, customer, new ReplaceOptions { IsUpsert = false }, ct);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct)
        {
            return Task.FromResult(0);
        }
    }
}
