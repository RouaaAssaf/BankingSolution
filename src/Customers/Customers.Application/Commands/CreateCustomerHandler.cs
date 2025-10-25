using Customers.Application.Interfaces;
using Customers.Domain.Entities;
using Banking.Messaging;
using Banking.Messaging.Events;
using MediatR;
using Customers.Domain.Exceptions;
using Microsoft.Extensions.Logging;


public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly ICustomerRepository _customers;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<CreateCustomerHandler> _logger;

    public CreateCustomerHandler(
        ICustomerRepository customers,
        IEventPublisher publisher,
        ILogger<CreateCustomerHandler> logger)
    {
        _customers = customers;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
      
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new DomainException("Email is required to create a customer.", 400);

        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new DomainException("First name is required.", 400);

        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new DomainException("Last name is required.", 400);

        var existing = await _customers.GetByEmailAsync(request.Email, ct);
        if (existing != null)
            throw new DomainException($"A customer with email '{request.Email}' already exists.", 409);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
        };

       
        await _customers.AddAsync(customer, ct);
        await _customers.SaveChangesAsync(ct);

        _logger.LogInformation("Customer {CustomerId} saved to DB", customer.Id);

        var evt = new CustomerCreatedEvent(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            DateTime.UtcNow,
            1);

        await _publisher.PublishAsync("customer.created", evt, ct);
        _logger.LogInformation("Published CustomerCreatedEvent for {CustomerId}", customer.Id);

        return customer.Id;
    }
}
