
using MediatR;
using Banking.Application.DTOs;
using System.Collections.Generic;

public record GetAllCustomersQuery() : IRequest<IEnumerable<CustomerDto>>;
