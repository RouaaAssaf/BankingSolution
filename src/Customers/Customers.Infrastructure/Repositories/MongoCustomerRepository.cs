using Customers.Application.Interfaces;
using Customers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;




    public class MongoCustomerRepository : ICustomerRepository
    {
        private readonly IMongoCollection<Customer> _customers;
      


        public MongoCustomerRepository(IMongoDatabase database)
        {
            _customers = database.GetCollection<Customer>("Customers");
          
        }

        public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            // Only fetch from Customers collection
            var customer = await _customers.Find(c => c.Id == id).FirstOrDefaultAsync(ct);
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

        public async Task DeleteAsync(Guid customerId, CancellationToken ct)
        {
            await _customers.DeleteOneAsync(c => c.Id == customerId, cancellationToken: ct);
        }
        public Task<int> SaveChangesAsync(CancellationToken ct)
        {
            return Task.FromResult(0);
        }
        public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken ct)
        {
            return await _customers.Find(_ => true).ToListAsync(ct);
        }
    }

