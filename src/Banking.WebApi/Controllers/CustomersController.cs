// src/Banking.WebApi/Controllers/CustomersController.cs
using Banking.Application.Customers;
using Microsoft.AspNetCore.Mvc;

namespace Banking.WebApi.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    [HttpGet("{id:guid}/summary", Name = "GetCustomerSummary")]
    public async Task<ActionResult<CustomerSummaryResponse>> GetSummary([FromServices] GetCustomerSummaryService svc, Guid id, CancellationToken ct)
        => await svc.HandleAsync(id, ct);
}
