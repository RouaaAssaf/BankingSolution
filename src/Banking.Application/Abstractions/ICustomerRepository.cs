using Banking.Domain.Entities;
namespace Banking.Application.Abstractions;
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct);
}