using Banking.Application.Customers;
using Banking.Application.Customers.Queries;
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
        return CreatedAtAction(nameof(GetSummary), new { id }, new { CustomerId = id });
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult<CustomerSummaryResponse>> GetSummary(Guid id, CancellationToken ct)
    {
        var query = new GetCustomerSummaryQuery(id);

        try
        {
            var summary = await _mediator.Send(query, ct);
            return Ok(summary);
        }
        catch (KeyNotFoundException ex) // thrown if customer not found
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
