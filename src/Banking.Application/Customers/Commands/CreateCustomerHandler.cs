using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Banking.Messaging;
using Banking.Messaging.Events;
using MediatR;
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
        // Validate input first

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required to create a customer.", nameof(request.Email));

        // Check if a customer with the same email already exists

        var existing = await _customers.GetByEmailAsync(request.Email, ct);
        if (existing != null)
            throw new InvalidOperationException($"A customer with email '{request.Email}' already exists.");



        // create domain entity
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            // optional fields like CreatedAt if you have them
        };

        //  persist to DB
        await _customers.AddAsync(customer, ct);
        await _customers.SaveChangesAsync(ct);

        _logger.LogInformation("Customer {CustomerId} saved to DB", customer.Id);

        // publish CustomerCreatedEvent (downstream services will create account)
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
