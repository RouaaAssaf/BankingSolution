using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using MediatR;

public class OpenAccountCommandHandler : IRequestHandler<OpenAccountCommand, Guid>
{
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactions;

    public OpenAccountCommandHandler(IAccountRepository accounts, ITransactionRepository transactions)
    {
        _accounts = accounts;
        _transactions = transactions;
    }

    public async Task<Guid> Handle(OpenAccountCommand request, CancellationToken ct)
    {
        // 1. Create account
        var account = new Account
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            OpenedAt = DateTime.UtcNow
        };

        await _accounts.AddAsync(account, ct);

        // 2. If initial credit > 0, add a transaction
        if (request.InitialDeposit > 0)
        {
            var tx = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                Amount = request.InitialDeposit,
                Type = TransactionType.Credit,
                Description = "Initial Credit",
                CreatedAt = DateTime.UtcNow
            };

            await _transactions.AddAsync(tx, ct);
        }

        return account.Id;
    }
}
