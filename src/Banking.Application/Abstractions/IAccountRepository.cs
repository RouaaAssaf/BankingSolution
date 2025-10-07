using Banking.Domain.Entities;

public interface IAccountRepository
{
    Task<int> GetTotalAccountsAsync(CancellationToken ct);
    Task<int> GetDistinctCustomerCountAsync(CancellationToken ct);
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct);
    Task<List<Account>> GetAccountsByCustomerIdAsync(Guid customerId, CancellationToken ct);
    Task<Account?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct);
    Task<Account> AddAsync(Account account, CancellationToken ct);
    Task<decimal> AddTransactionAsync(Transaction transaction, CancellationToken ct);
    Task<int> CountActiveCustomersAsync(CancellationToken ct);
    Task<int> CountAccountsWithAlertAsync(CancellationToken ct);
    Task<int> CountVerifiedAccountsAsync(CancellationToken ct);

    Task<int> SaveChangesAsync(CancellationToken ct);
    Task UpdateAsync(Account account, CancellationToken ct);
}


