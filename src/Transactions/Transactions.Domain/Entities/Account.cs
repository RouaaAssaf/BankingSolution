

namespace Transactions.Domain.Entities;

public class Account
{
    
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public decimal Balance { get; set; }

    public bool IsVerified { get; set; } = false;   // True if account is verified
    public bool IsActive { get; set; } = true;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

}

