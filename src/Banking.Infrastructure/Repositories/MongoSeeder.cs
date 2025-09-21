using Banking.Domain.Entities;
using MongoDB.Driver;

namespace Banking.Infrastructure.Data;

public static class MongoSeeder
{
    public static async Task SeedAsync(IMongoDatabase database, CancellationToken ct = default)
    {
        var customers = database.GetCollection<Customer>("Customers");

        // Check if already seeded
        var count = await customers.CountDocumentsAsync(FilterDefinition<Customer>.Empty, cancellationToken: ct);
        if (count == 0)
        {
            var initialCustomers = new[]
            {
                new Customer { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), FirstName = "Alice", LastName = "Anderson" },
                new Customer { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), FirstName = "Bob", LastName = "Brown" }
            };

            await customers.InsertManyAsync(initialCustomers, cancellationToken: ct);
        }
    }
}
