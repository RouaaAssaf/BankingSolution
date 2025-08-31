using Banking.Application.Customers;
using Microsoft.AspNetCore.Mvc;

namespace Banking.WebApi.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly GetCustomerSummaryService _service;

    public CustomersController(GetCustomerSummaryService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult<CustomerSummaryResponse>> GetSummary(Guid id, CancellationToken ct)
    {
        var summary = await _service.HandleAsync(id, ct);
        return Ok(summary);
    }
}
