
using MongoDB.Bson.Serialization.Attributes;

namespace Banking.Domain.Entities;
public class Customer
{
    [BsonId]
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Email { get; set; }
    public decimal Balance { get; set; } = 0m;
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}