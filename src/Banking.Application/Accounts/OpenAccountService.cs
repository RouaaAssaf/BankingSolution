// src/Banking.Application/Accounts/OpenAccountService.cs
using Banking.Application.Abstractions;
using Banking.Domain.Entities;

namespace Banking.Application.Accounts;
public class OpenAccountService
{
    private readonly ICustomerRepository _customers;
    private readonly IAccountRepository _accounts;

    public OpenAccountService(ICustomerRepository customers, IAccountRepository accounts)
    {
        _customers = customers;
        _accounts = accounts;
    }

    public async Task<Guid> HandleAsync(OpenAccountRequest request, CancellationToken ct = default)
    {
        // 1. Check if customer exists
        var customer = await _customers.GetByIdAsync(request.CustomerId, ct)
                      ?? throw new InvalidOperationException("Customer not found");

        // 2. Create a new account
        var account = await _accounts.AddAsync(new Account
        {
            Id = Guid.NewGuid(),            //Generate new account Id here
            CustomerId = customer.Id,
        }, ct);

        // 3. Add initial credit if provided
        if (request.InitialCredit > 0)
        {
            await _accounts.AddTransactionAsync(new Transaction
            {
                AccountId = account.Id,
                Amount = Math.Abs(request.InitialCredit),
                Type = TransactionType.Credit,
                Description = "Initial credit"
            }, ct);
        }

        // 4. Save all changes
        await _accounts.SaveChangesAsync(ct);

        return account.Id;
    }
}