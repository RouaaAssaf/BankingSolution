using Banking.Application.Accounts.Commands;
using Banking.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Banking.Application.Accounts;



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

    [HttpPost]
    public async Task<IActionResult> OpenAccount(
    [FromBody] OpenAccountRequest request,
    CancellationToken ct)
    {
         try
        {
            var command = new OpenAccountCommand(
                request.CustomerId,
                request.InitialCredit
            );

            var accountId = await _mediator.Send(command, ct);

            return Created("", new { AccountId = accountId });
        }
        catch (InvalidOperationException ex) when (ex.Message == "Customer not found")
        {
            return NotFound(new { Message = ex.Message });
        }
    }


    [HttpPost("{accountId:guid}/transaction")]
    public async Task<IActionResult> AddTransaction(
    Guid accountId,
    [FromBody] AddTransactionRequest request,
    CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(TransactionType), request.TransactionType))
            return BadRequest("Invalid transaction type. Allowed values are 0 (Credit) or 1 (Debit).");


        // Map DTO → Command
        var command = new AddTransactionCommand(
            accountId,
            request.Amount,
            request.TransactionType,
            request.Description
        );

        // Send command to MediatR
        var txId = await _mediator.Send(command, ct);

        return Created("", new { TransactionId = txId });
    }
}