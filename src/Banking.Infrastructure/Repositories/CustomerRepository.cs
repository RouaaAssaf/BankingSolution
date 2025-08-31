using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Banking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Repositories;
public class CustomerRepository : ICustomerRepository
{
    private readonly BankingDbContext _db;
    public CustomerRepository(BankingDbContext db) => _db = db;

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Customers
           .Include(c => c.Accounts)
               .ThenInclude(a => a.Transactions)
           .FirstOrDefaultAsync(c => c.Id == id, ct);
}