

namespace Customers.Application.DTOs
{
   
    public record CustomerDto(Guid CustomerId, string FirstName, string LastName, string Email);
}
