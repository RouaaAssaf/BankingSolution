using Banking.Domain.Entities;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct);
    Task<Account?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct);
    Task<Account> AddAsync(Account account, CancellationToken ct);
    Task<decimal> AddTransactionAsync(Transaction transaction, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
    Task UpdateAsync(Account account, CancellationToken ct);
}


