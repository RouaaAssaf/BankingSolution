namespace Banking.Domain.Entities;
public enum TransactionType { Credit, Debit }

public class Transaction
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;  // 👈 new navigation property
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string Description { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
