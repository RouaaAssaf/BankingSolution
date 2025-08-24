// src/Banking.Infrastructure/Data/BankingDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Banking.Infrastructure.Data;

public class BankingDbContextFactory : IDesignTimeDbContextFactory<BankingDbContext>
{
    public BankingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BankingDbContext>();
        optionsBuilder.UseSqlite("Data Source=banking.db"); // same as in appsettings.json

        return new BankingDbContext(optionsBuilder.Options);
    }
}
