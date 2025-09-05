using Banking.Application.Abstractions;
using MediatR;

namespace Banking.Application.Customers.Queries;

public class GetCustomerSummaryQueryHandler : IRequestHandler<GetCustomerSummaryQuery, CustomerSummaryResponse>
{
    private readonly ICustomerRepository _customers;

    public GetCustomerSummaryQueryHandler(ICustomerRepository customers)
    {
        _customers = customers;
    }

    public async Task<CustomerSummaryResponse> Handle(GetCustomerSummaryQuery request, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(request.CustomerId, ct);
        if (customer == null)
            throw new KeyNotFoundException($"Customer with ID {request.CustomerId} not found.");


        var allTx = customer.Accounts
                            .SelectMany(a => a.Transactions)
                            .OrderByDescending(t => t.CreatedAt)
                            .Select(t => new TransactionDto(t.Id, t.AccountId, t.Amount, t.Type.ToString(), t.Description, t.CreatedAt))
                            .ToList();

        var total = customer.Accounts.Sum(a => a.Balance);

        return new CustomerSummaryResponse(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            total,
            allTx
        );
    }
} 