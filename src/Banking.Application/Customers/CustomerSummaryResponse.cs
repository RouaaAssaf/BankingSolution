namespace Banking.Application.Customers;
public record TransactionDto(Guid Id, Guid AccountId, decimal Amount, string Type, string Description, DateTime CreatedAt);
public record CustomerSummaryResponse(Guid CustomerId, string FirstName, string LastName, decimal TotalBalance, IReadOnlyList<TransactionDto> Transactions);