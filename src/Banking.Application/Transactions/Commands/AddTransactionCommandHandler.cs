using MediatR;
using Banking.Domain.Entities;

public class AddTransactionHandler : IRequestHandler<AddTransactionCommand, Guid>
{
    private readonly IAccountRepository _accounts;

    public AddTransactionHandler(IAccountRepository accounts)
    {
        _accounts = accounts;
    }

    public async Task<Guid> Handle(AddTransactionCommand request, CancellationToken cancellationToken)
    {
        //  Get the account from database
        var account = await _accounts.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null)
            throw new Exception("Account not found");

        //  Create a new transaction
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Amount = request.Amount,
            Description = request.Description,
            Type = request.Type,
            CreatedAt = DateTime.UtcNow
        };

        //  Add transaction to account
        await _accounts.AddTransactionAsync(transaction, cancellationToken);

        //  Save changes
        await _accounts.SaveChangesAsync(cancellationToken);

        return transaction.Id;
    }
}
