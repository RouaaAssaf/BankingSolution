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
}

