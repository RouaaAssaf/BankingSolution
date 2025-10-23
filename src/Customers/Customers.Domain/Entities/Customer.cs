

namespace Customers.Domain.Entities;
public class Customer
{
   
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Email { get; set; }
    public decimal Balance { get; set; } = 0m;
   
}