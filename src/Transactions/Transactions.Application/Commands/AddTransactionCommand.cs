using Transactions.Domain.Entities;
using MediatR;

public record AddTransactionCommand(
    Guid AccountId,
    decimal Amount,
    TransactionType Type,
    string Description
) : IRequest<Guid>;

