using MongoDB.Bson.Serialization.Attributes;

namespace Banking.Domain.Entities;
[BsonIgnoreExtraElements]
public class Account
{
    [BsonId]
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

