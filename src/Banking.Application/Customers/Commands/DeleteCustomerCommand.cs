using MediatR;

namespace Banking.Application.Customers.Commands
{
    public record DeleteCustomerCommand(Guid CustomerId) : IRequest<Unit>;
}
