using MediatR;

namespace Customers.Application.Customers.Commands
{
    public record DeleteCustomerCommand(Guid CustomerId) : IRequest<Unit>;
}
