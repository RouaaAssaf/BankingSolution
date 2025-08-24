using Banking.Domain.Entities;

namespace Banking.Application.Accounts;

public record AddTransactionRequest(decimal Amount, TransactionType Type, string Description);
