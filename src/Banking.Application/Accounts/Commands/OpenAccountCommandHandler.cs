using Banking.Domain.Entities;
using MediatR;

public class OpenAccountHandler : IRequestHandler<OpenAccountCommand, Guid>
{
    private readonly IAccountRepository _accounts;

    public OpenAccountHandler(IAccountRepository accounts)
    {
        _accounts = accounts;
    }

    public async Task<Guid> Handle(OpenAccountCommand request, CancellationToken cancellationToken)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId
        };

        if (request.InitialDeposit > 0)
        {
            account.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                Amount = request.InitialDeposit,
                Description = "Initial Deposit",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _accounts.AddAsync(account, cancellationToken);
        await _accounts.SaveChangesAsync(cancellationToken);
        return account.Id;
    }
}
