// Banking.Application.Handlers/GetAllCustomersQueryHandler.cs
using MediatR;
using Customers.Application.DTOs;
using Customers.Application.Interfaces;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, IEnumerable<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepo;

    public GetAllCustomersQueryHandler(ICustomerRepository customerRepo)
    {
        _customerRepo = customerRepo;
    }

    public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken ct)
    {
        var customers = await _customerRepo.GetAllAsync(ct); // Repository fetches all customers
        return customers.Select(c => new CustomerDto(
            c.Id, 
            c.FirstName,
            c.LastName,
            c.Email
        ));
    }
}
