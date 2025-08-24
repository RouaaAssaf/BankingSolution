namespace Banking.Domain.Entities;
public class Account
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;  // 👈 new navigation property
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public decimal Balance => Transactions.Sum(t => t.Amount);
}
