
namespace Banking.Messaging.Events;

public record CustomerCreatedEvent(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string? Email,
    DateTime OccurredAt,
    int Version = 1
);

public record AccountCreatedEvent(
    Guid AccountId,
    Guid CustomerId,
    decimal InitialBalance,
    DateTime CreatedAt,
    int Version = 1
);

public record TransactionCreatedEvent(
    Guid TransactionId,
    Guid AccountId,
    Guid CustomerId,
    decimal Amount,
    string TransactionType,
    string Description,
    DateTime OccurredAt,
    int Version = 1
);

public record AccountBalanceUpdatedEvent(
    Guid AccountId,
    Guid CustomerId,
    decimal NewBalance,
    DateTime UpdatedAt,
    int Version = 1
);
