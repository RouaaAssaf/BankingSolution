using static CustomerSummaryResponse;

public record CustomerSummaryResponse(
    Guid CustomerId,
    string FirstName,
    string LastName,
    IEnumerable<AccountDto> Accounts
)
{
    
    public record AccountDto(
        Guid AccountId,
        decimal Balance,
        DateTime OpenedAt,
        IEnumerable<TransactionDto> Transactions
    );

    public record TransactionDto(
        Guid TransactionId,
        decimal Amount,
        string TransactionType,
        string Description,
        DateTime CreatedAt
    );
    public record AccountInfoResponse(
    Guid AccountId,
    Guid CustomerId,
    decimal Balance,
    DateTime OpenedAt,
    string FirstName,
    string LastName
    );
}
