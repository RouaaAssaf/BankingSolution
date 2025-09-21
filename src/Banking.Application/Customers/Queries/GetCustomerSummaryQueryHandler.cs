using Banking.Application.Abstractions;
using MediatR;

public class GetCustomerSummaryQueryHandler : IRequestHandler<GetCustomerSummaryQuery, CustomerSummaryResponse?>
{
    private readonly IAccountRepository _accountRepo;
    private readonly ITransactionRepository _transactionRepo;

    public GetCustomerSummaryQueryHandler(
        IAccountRepository accountRepo,
        ITransactionRepository transactionRepo)
    {
        _accountRepo = accountRepo;
        _transactionRepo = transactionRepo;
    }

    public async Task<CustomerSummaryResponse?> Handle(GetCustomerSummaryQuery request, CancellationToken ct)
    {
        //Get all accounts for the customer
        var accounts = await _accountRepo.GetAccountsByCustomerIdAsync(request.CustomerId, ct);
        if (accounts == null || !accounts.Any()) return null;

        // Get all transactions for these accounts
        var accountIds = accounts.Select(a => a.Id);
        var transactions = await _transactionRepo.GetByAccountIdsAsync(accountIds, ct);

        // Map to CustomerSummaryResponse with Accounts + Transactions
        var accountsDto = accounts.Select(a => new CustomerSummaryResponse.AccountDto(
            AccountId: a.Id,
            Balance: a.Balance,
            OpenedAt: a.OpenedAt,
            Transactions: transactions
                .Where(t => t.AccountId == a.Id)
                .Select(t => new CustomerSummaryResponse.TransactionDto(
                    TransactionId: t.Id,
                    Amount: t.Amount,
                    TransactionType: t.Type.ToString(),
                    Description: t.Description,
                    CreatedAt: t.CreatedAt
                ))
                .OrderByDescending(t => t.CreatedAt)
        ));

        //  Calculate total balance
        var totalBalance = accounts.Sum(a => a.Balance);

        //  Return final summary
        var firstAccount = accounts.First();
        return new CustomerSummaryResponse(
            CustomerId: request.CustomerId,
            FirstName: firstAccount.FirstName,
            LastName: firstAccount.LastName,
            Accounts: accountsDto

        );
    }
}
