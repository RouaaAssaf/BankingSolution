using Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class BankingDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; } = default!;
    public DbSet<Account> Accounts { get; set; } = default!;
    public DbSet<Transaction> Transactions { get; set; } = default!;

    public BankingDbContext(DbContextOptions<BankingDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Account → Customer
        modelBuilder.Entity<Account>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Accounts)
            .HasForeignKey(a => a.CustomerId);

        // Transaction → Account
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId);

        base.OnModelCreating(modelBuilder);
    }
}
