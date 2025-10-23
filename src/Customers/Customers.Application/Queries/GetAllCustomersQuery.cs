
using MediatR;
using Customers.Application.DTOs;


public record GetAllCustomersQuery() : IRequest<IEnumerable<CustomerDto>>;
