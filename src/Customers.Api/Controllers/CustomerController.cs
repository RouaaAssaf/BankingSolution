
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Banking.WebApi.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // POST /api/customers
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var command = new CreateCustomerCommand(request.FirstName, request.LastName, request.Email);
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(null, new { id }, new { CustomerId = id });
    }
}
