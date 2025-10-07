using Banking.Domain.Entities;
namespace Banking.Application.Abstractions;
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct);
    Task UpdateBalanceAsync(Guid customerId, decimal newBalance, CancellationToken ct);
    Task<Customer> AddAsync(Customer customer, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
    Task<IEnumerable<Customer>> GetAllAsync(CancellationToken ct);
}