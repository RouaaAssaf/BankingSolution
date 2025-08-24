// src/Banking.Infrastructure/Data/DbSeeder.cs
using Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Data;
public static class DbSeeder
{
    public static async Task SeedAsync(BankingDbContext db, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);
        if (!db.Customers.Any())
        {
            db.Customers.AddRange(
                new Customer { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), FirstName = "Alice", LastName = "Anderson" },
                new Customer { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), FirstName = "Bob", LastName = "Brown" }
            );
            await db.SaveChangesAsync(ct);
        }
    }
}
