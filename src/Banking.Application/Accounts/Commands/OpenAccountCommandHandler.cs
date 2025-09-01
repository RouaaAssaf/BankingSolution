using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using MediatR;

namespace Banking.Application.Accounts.Commands;

public class OpenAccountCommandHandler : IRequestHandler<OpenAccountCommand, Guid>
{
    private readonly ICustomerRepository _customers;
    private readonly IAccountRepository _accounts;

    public OpenAccountCommandHandler(ICustomerRepository customers, IAccountRepository accounts)
    {
        _customers = customers;
        _accounts = accounts;
    }

    public async Task<Guid> Handle(OpenAccountCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate customer exists
        var customer = await _customers.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            throw new InvalidOperationException("Customer not found");

        // 2. Create new account
        var account = new Account
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId
        };

        // 3. Add initial transaction if deposit > 0
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

        // 4. Save account
        await _accounts.AddAsync(account, cancellationToken);
        await _accounts.SaveChangesAsync(cancellationToken);

        return account.Id;
    }
}
