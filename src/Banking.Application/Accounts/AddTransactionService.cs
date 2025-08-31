using Banking.Application.Abstractions;
using Banking.Application.Accounts;
using Banking.Domain.Entities;


public class AddTransactionService
{
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactions;

    public AddTransactionService(IAccountRepository accounts, ITransactionRepository transactions)
    {
        _accounts = accounts;
        _transactions = transactions;
    }

    public async Task<Guid> HandleAsync(Guid accountId, AddTransactionRequest request, CancellationToken ct)
    {
        var account = await _accounts.GetByIdAsync(accountId, ct)
            ?? throw new InvalidOperationException("Account not found.");

        var signed = request.TransactionType == TransactionType.Debit
            ? -Math.Abs(request.Amount)
            : Math.Abs(request.Amount);

        var tx = new Transaction
        {
            AccountId = accountId,
            Amount = signed,
            Type = request.TransactionType,
            Description = request.Description
        };

        await _transactions.AddAsync(tx, ct);
        await _transactions.SaveChangesAsync(ct);
        return tx.Id;
    }


}