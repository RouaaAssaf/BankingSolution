using Banking.Domain.Entities;

public class AccountRepository : IAccountRepository
{
    private readonly BankingDbContext _context;
    public AccountRepository(BankingDbContext context) => _context = context;

    public async Task<Account> AddAsync(Account account, CancellationToken ct)
    {
        var entry = await _context.Accounts.AddAsync(account, ct);
        return entry.Entity;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _context.Accounts.FindAsync(new object?[] { id }, ct);

    public Task AddTransactionAsync(Transaction transaction, CancellationToken ct)
    {
        _context.Transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct)
        => await _context.SaveChangesAsync(ct);
}