using Banking.Domain.Entities;
using Banking.Messaging;
using Banking.Messaging.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Banking.Application.Transactions.Commands;

public class AddTransactionHandler : IRequestHandler<AddTransactionCommand, Guid>
{
    private readonly IAccountRepository _accounts;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<AddTransactionHandler> _logger;

    public AddTransactionHandler(
        IAccountRepository accounts,
        IEventPublisher publisher,
        ILogger<AddTransactionHandler> logger)
    {
        _accounts = accounts;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Guid> Handle(AddTransactionCommand request, CancellationToken ct)
    {
        // 🔹 1. Validate Account
        var account = await _accounts.GetByIdAsync(request.AccountId, ct);
        if (account == null)
        {
            _logger.LogWarning("Account {AccountId} not found", request.AccountId);
            throw new KeyNotFoundException($"Account with ID {request.AccountId} not found.");
        }

        // 🔹 2. Validate Amount
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.");
        }

        // 🔹 3. Create Transaction Object
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Amount = request.Amount,
            Description = request.Description,
            Type = request.Type,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Created transaction {TransactionId} for Account {AccountId}", transaction.Id, account.Id);

        // 🔹 4. Update Account and Insert Transaction
        var updatedBalance = await _accounts.AddTransactionAsync(transaction, ct);

        // 🔹 5. Publish Event
        var transactionEvent = new TransactionCreatedEvent(
            transaction.Id,
            transaction.AccountId,
            account.CustomerId,
            transaction.Amount,
            transaction.Type.ToString(),
            transaction.Description,
            transaction.CreatedAt
        );
        await _publisher.PublishAsync("transaction.created", transactionEvent, ct);

        return transaction.Id;
    }
}
