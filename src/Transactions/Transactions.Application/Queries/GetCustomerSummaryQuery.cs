using MediatR;

public record GetCustomerSummaryQuery(Guid CustomerId) : IRequest<CustomerSummaryResponse?>;
