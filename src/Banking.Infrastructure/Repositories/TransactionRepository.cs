using Microsoft.EntityFrameworkCore;
using Banking.Domain.Entities;
using Banking.Application.Abstractions;


public class TransactionRepository : ITransactionRepository
{
    private readonly BankingDbContext _context;
    public TransactionRepository(BankingDbContext context) => _context = context;

    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken ct)
        => await _context.Transactions
                         .Where(t => t.AccountId == accountId)
                         .ToListAsync(ct);

    public Task AddAsync(Transaction transaction, CancellationToken ct)
    {
        _context.Transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return await _context.SaveChangesAsync(ct);
    }

}
