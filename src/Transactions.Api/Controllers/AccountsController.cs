using Banking.Application.Accounts;
using Banking.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;


namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Only transaction endpoint remains
    [HttpPost("{accountId:guid}/transaction")]
    public async Task<IActionResult> AddTransaction(
        Guid accountId,
        [FromBody] AddTransactionRequest request,
        CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(TransactionType), request.TransactionType))
            return BadRequest("Invalid transaction type. Allowed values are 0 (Credit) or 1 (Debit).");

        try
        {
            
            var command = new AddTransactionCommand(
                accountId,
                request.Amount,
                request.TransactionType,
                request.Description
            );

            var txId = await _mediator.Send(command, ct);

            return Created("", new
            {
                Message = "Transaction added successfully",
                TransactionId = txId
            });
        }
        catch (KeyNotFoundException ex) // account not found
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (ArgumentException ex) // validation error (e.g. invalid amount)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("customers/{customerId:guid}/summary")]
    public async Task<IActionResult> GetCustomerSummary(Guid customerId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCustomerSummaryQuery(customerId), ct);
        if (result == null) return NotFound(new { Message = "Customer not found" });
        return Ok(result);
    }

}
