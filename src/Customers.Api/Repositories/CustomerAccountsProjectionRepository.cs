
using Customers.Api.Projections;
using MongoDB.Driver;

namespace Customers.Api.Repositories;

public class CustomerAccountsProjectionRepository
{
    private readonly IMongoCollection<CustomerAccountsProjection> _collection;
    private readonly ILogger<CustomerAccountsProjectionRepository> _logger;

    public CustomerAccountsProjectionRepository(IMongoDatabase db, ILogger<CustomerAccountsProjectionRepository> logger)
    {
        _collection = db.GetCollection<CustomerAccountsProjection>("CustomerAccountsProjection");
        _logger = logger;
    }

    public async Task AddAccountAsync(Guid customerId, Guid accountId, decimal balance, DateTime createdAt, CancellationToken ct)
    {
        var filter = Builders<CustomerAccountsProjection>.Filter.Eq(p => p.CustomerId, customerId);
        var update = Builders<CustomerAccountsProjection>.Update.Push(p => p.Accounts,
            new CustomerAccountsProjection.AccountInfo
            {
                AccountId = accountId,
                Balance = balance,
                CreatedAt = createdAt
            });

        await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task UpdateBalanceAsync(Guid customerId, Guid accountId, decimal newBalance, CancellationToken ct)
    {
        var filter = Builders<CustomerAccountsProjection>.Filter.And(
            Builders<CustomerAccountsProjection>.Filter.Eq(p => p.CustomerId, customerId),
            Builders<CustomerAccountsProjection>.Filter.ElemMatch(p => p.Accounts, a => a.AccountId == accountId)
        );

        var update = Builders<CustomerAccountsProjection>.Update.Set("Accounts.$.Balance", newBalance);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);

        if (result.ModifiedCount > 0)
            _logger.LogInformation("Projection updated for CustomerId={CustomerId}, AccountId={AccountId}, NewBalance={Balance}", customerId, accountId, newBalance);
        else
            _logger.LogWarning("Projection NOT updated! CustomerId={CustomerId}, AccountId={AccountId}", customerId, accountId);
    }
}
