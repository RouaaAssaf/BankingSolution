
using Banking.Domain.Entities;
namespace Banking.Application.Abstractions;

public interface ITransactionRepository

{
    Task<int> CountTransactionsAsync(Func<Transaction, bool>? predicate = null, CancellationToken ct = default);
   

    Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken ct);
    Task<IEnumerable<Transaction>> GetByAccountIdsAsync(IEnumerable<Guid> accountIds, CancellationToken ct);
    Task AddAsync(Transaction transaction, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
