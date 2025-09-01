using MediatR;

namespace Banking.Application.Customers.Queries;

public record GetCustomerSummaryQuery(Guid CustomerId) : IRequest<CustomerSummaryResponse>;
