
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Customers.Application.Customers.Commands;
namespace Customers.WebApi.Controllers;

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
        return CreatedAtAction(null, new { id }, new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var customers = await _mediator.Send(new GetAllCustomersQuery(), ct);
        return Ok(customers);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken ct)
    {
        var command = new DeleteCustomerCommand(id);
        await _mediator.Send(command, ct);
        return NoContent();
    }


}
