
using MongoDB.Bson.Serialization.Attributes;


namespace Customers.Api.Projections;

public class CustomerAccountsProjection
{
    [BsonId]
    public Guid CustomerId { get; set; }

    public List<AccountInfo> Accounts { get; set; } = new();

    public class AccountInfo
    {
        public Guid AccountId { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
