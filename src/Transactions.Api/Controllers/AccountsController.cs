using Banking.Application.Abstractions;
using Banking.Application.DTOs;
using Banking.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static CustomerSummaryResponse;




namespace Transactions.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAccountRepository _accountRepo;
    private readonly ITransactionRepository _transactionRepo;

    public AccountsController(IMediator mediator,IAccountRepository accountRepo, ITransactionRepository transactionRepo)
    {
        _mediator = mediator;
        _accountRepo = accountRepo;
        _transactionRepo = transactionRepo;
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

    [HttpGet("{accountId:guid}")]
    public async Task<IActionResult> GetAccountById(Guid accountId, CancellationToken ct)
    {
        var account = await _accountRepo.GetByIdAsync(accountId, ct);

        if (account == null)
            return NotFound(new { Message = "Account not found" });

        return Ok(new AccountInfoResponse(
            AccountId: account.Id,
            CustomerId: account.CustomerId,
            Balance: account.Balance,
            OpenedAt: account.OpenedAt,
            FirstName: account.FirstName,
            LastName: account.LastName
       ));
    }
    [HttpGet("/api/dashboard/summary")]
    public async Task<IActionResult> GetDashboardSummary(CancellationToken ct)
    {
        var totalAccounts = await _accountRepo.GetTotalAccountsAsync(ct);
        var totalCustomers = await _accountRepo.GetDistinctCustomerCountAsync(ct);

        var today = DateTime.UtcNow.Date;
        var totalTransactionsToday = await _transactionRepo.CountTransactionsAsync(t => t.CreatedAt >= today, ct);

        // NEW: Active customers
        var activeCustomers = await _accountRepo.CountActiveCustomersAsync(ct);

        // NEW: Pending transactions
        var pendingTransactions = await _transactionRepo.CountTransactionsAsync(t => t.Status == TransactionStatus.Pending, ct);

        // NEW: Accounts with alerts (e.g., balance < threshold)
        var alertAccounts = await _accountRepo.CountAccountsWithAlertAsync(ct);

        var dailyTransactionGoal = 50;

        // NEW: Verified accounts
        var verifiedAccounts = await _accountRepo.CountVerifiedAccountsAsync(ct);

        return Ok(new DashboardSummaryDto
        {
            TotalCustomers = totalCustomers,
            TotalAccounts = totalAccounts,
            TransactionsToday = totalTransactionsToday,
            ActiveCustomers = activeCustomers,
            PendingTransactions = pendingTransactions,
            AlertAccounts = alertAccounts,
            DailyTransactionGoal = dailyTransactionGoal,
            VerifiedAccounts = verifiedAccounts
        });

    }






}
