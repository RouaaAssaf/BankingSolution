// src/Banking.Application/Customers/GetCustomerSummaryService.cs
using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Banking.Application.Customers;
public class GetCustomerSummaryService
{
    private readonly ICustomerRepository _customers;
    private readonly ILogger<GetCustomerSummaryService> _logger;

    public GetCustomerSummaryService(ICustomerRepository customers, ILogger<GetCustomerSummaryService> logger)
    {
        _customers = customers; 
        _logger = logger;
    }

    public async Task<CustomerSummaryResponse> HandleAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _customers.GetByIdAsync(customerId, ct)
                      ?? throw new InvalidOperationException("Customer not found.");

        var allTx = customer.Accounts.SelectMany(a => a.Transactions)
                     .OrderByDescending(t => t.CreatedAt)
                     .Select(t => new TransactionDto(t.Id, t.AccountId, t.Amount, t.Type.ToString(), t.Description, t.CreatedAt))
                     .ToList();

        var total = customer.Accounts.Sum(a => a.Balance);

        return new CustomerSummaryResponse(customer.Id, customer.FirstName, customer.LastName, total, allTx);
    }
}
