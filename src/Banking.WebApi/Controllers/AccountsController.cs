using Banking.Application.Accounts;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly OpenAccountService _openAccountService;
    private readonly AddTransactionService _addTransactionService;

    public AccountsController(OpenAccountService openAccountService, AddTransactionService addTransactionService)
    {
        _openAccountService = openAccountService;
        _addTransactionService = addTransactionService;
    }

    [HttpPost("open")]
    public async Task<IActionResult> OpenAccount([FromBody] OpenAccountRequest request, CancellationToken ct)
    {
        var accountId = await _openAccountService.HandleAsync(request, ct);
        return CreatedAtAction(nameof(OpenAccount), new { accountId });
    }

    [HttpPost("{accountId:guid}/transaction")]
    public async Task<IActionResult> AddTransaction(Guid accountId,
     [FromBody] AddTransactionRequest request, CancellationToken ct)
    {
        var txId = await _addTransactionService.HandleAsync(accountId, request, ct);
        return CreatedAtAction(nameof(AddTransaction), new { txId }, new { txId });
    }

}
