using Banking.Application.Abstractions;
using Banking.Application.Customers.Commands;
using Banking.Messaging;
using Banking.Messaging.Events;
using MediatR;
using Microsoft.Extensions.Logging;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Unit>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IAccountRepository _accountRepo;
    private readonly ITransactionRepository _transactionRepo; // Add this
    private readonly IEventPublisher _publisher;
    private readonly ILogger<DeleteCustomerCommandHandler> _logger;

    public DeleteCustomerCommandHandler(
        ICustomerRepository customerRepo,
        IAccountRepository accountRepo,
        ITransactionRepository transactionRepo, // Inject transaction repo
        IEventPublisher publisher,
        ILogger<DeleteCustomerCommandHandler> logger)
    {
        _customerRepo = customerRepo;
        _accountRepo = accountRepo;
        _transactionRepo = transactionRepo; // Assign
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteCustomerCommand request, CancellationToken ct)
    {
        var customer = await _customerRepo.GetByIdAsync(request.CustomerId, ct);
        if (customer == null)
        {
            _logger.LogWarning("Attempted to delete non-existent customer {CustomerId}", request.CustomerId);
            return Unit.Value;
        }

        var accounts = await _accountRepo.GetAccountsByCustomerIdAsync(request.CustomerId, ct);

        foreach (var account in accounts)
        {
            // Delete all transactions for this account
            await _transactionRepo.DeleteByAccountIdAsync(account.Id, ct);

            // Then delete the account
            await _accountRepo.DeleteAsync(account.Id, ct);

            _logger.LogInformation("Deleted Account {AccountId} and its transactions for Customer {CustomerId}", account.Id, request.CustomerId);
        }

        // Delete the customer
        await _customerRepo.DeleteAsync(request.CustomerId, ct);
        _logger.LogInformation("Deleted Customer {CustomerId}", request.CustomerId);

        // Publish event
        var evt = new CustomerDeletedEvent(request.CustomerId);
        await _publisher.PublishAsync("customer.deleted", evt, ct);
        _logger.LogInformation("✅ Published CustomerDeletedEvent for Customer {CustomerId}", request.CustomerId);

        return Unit.Value;
    }
}
