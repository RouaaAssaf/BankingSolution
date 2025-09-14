
using Banking.Domain.Entities;
using Banking.Messaging;
using Banking.Messaging.Events;
using MediatR;
using Microsoft.Extensions.Logging;

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

        _logger.LogInformation("Processing AddTransactionCommand for AccountId={AccountId}, Amount={Amount}, Type={Type}",
            request.AccountId, request.Amount, request.Type);

        // Get the account
        var account = await _accounts.GetByIdAsync(request.AccountId, ct);
        if (account == null)
        {
            _logger.LogWarning("Account {AccountId} not found", request.AccountId);
            throw new Exception("Account not found");
        }        

        // Create a transaction (Id always new!)
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

        // Insert transaction + update balance atomically
        var updatedBalance = await _accounts.AddTransactionAsync(transaction, ct);


        // Publish TransactionCreatedEvent
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
        _logger.LogInformation("Published TransactionCreatedEvent for TransactionId={TransactionId}", transaction.Id);

        // Publish AccountBalanceUpdatedEvent
        var balanceEvent = new AccountBalanceUpdatedEvent(
            account.Id,
            account.CustomerId,
            updatedBalance,
            DateTime.UtcNow
        );
        await _publisher.PublishAsync("account.balance.updated", balanceEvent, ct);
        _logger.LogInformation("Published AccountBalanceUpdatedEvent for AccountId={AccountId} with Balance={Balance}", account.Id, updatedBalance);

        //  Return transaction Id to caller
        return transaction.Id;
    }
}
