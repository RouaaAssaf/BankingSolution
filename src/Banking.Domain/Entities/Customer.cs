namespace Banking.Domain.Entities;
public class Customer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Email { get; set; }
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}