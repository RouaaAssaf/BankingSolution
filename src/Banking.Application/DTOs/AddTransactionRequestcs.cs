using Banking.Domain.Entities;

namespace Banking.Application.DTOs;


public class AddTransactionRequest
{
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; } = default!;
    public string Description { get; set; } = default!;
}
