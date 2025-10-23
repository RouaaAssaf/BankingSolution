
using Banking.Messaging;
using Banking.Messaging.Events;
using Customers.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
namespace Customers.Application.Customers.Commands;


public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Unit>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<DeleteCustomerCommandHandler> _logger;

    public DeleteCustomerCommandHandler(
        ICustomerRepository customerRepo,
        IEventPublisher publisher,
        ILogger<DeleteCustomerCommandHandler> logger)
    {
        _customerRepo = customerRepo;
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

        await _customerRepo.DeleteAsync(request.CustomerId, ct);
        _logger.LogInformation("Deleted Customer {CustomerId}", request.CustomerId);

        // Publish event for other services (Accounts, Transactions)
        var evt = new CustomerDeletedEvent(request.CustomerId);
        await _publisher.PublishAsync("customer.deleted", evt, ct);
        _logger.LogInformation("✅ Published CustomerDeletedEvent for Customer {CustomerId}", request.CustomerId);

        return Unit.Value;
    }
}
