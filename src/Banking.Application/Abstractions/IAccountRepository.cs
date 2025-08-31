using Banking.Domain.Entities;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct); // Add this
    Task<Account> AddAsync(Account account, CancellationToken ct);
    Task AddTransactionAsync(Transaction transaction, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}


